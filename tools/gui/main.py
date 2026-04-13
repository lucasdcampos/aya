import pygame
import chess
import chess.engine
import os
import sys
import argparse
import threading
import queue

from constants import DEFAULT_ENGINE_PATH, WIDTH, HEIGHT, WINDOW_WIDTH, SQ_SIZE
from renderer import load_images, draw_board, draw_highlights, draw_pieces
from ui import draw_sidebar, draw_game_over

def parse_args():
    parser = argparse.ArgumentParser(description="Aya Chess Engine GUI")
    parser.add_argument("--engine", type=str, default=None, help="Path to the UCI engine executable")
    parser.add_argument("--mode", choices=["human-vs-engine", "engine-vs-engine"], default="human-vs-engine", help="Game mode")
    return parser.parse_args()

class EngineThread(threading.Thread):
    """Thread to handle engine calculations without blocking the UI."""
    def __init__(self, engine, board, result_queue, limit):
        super().__init__()
        self.engine = engine
        self.board = board.copy()
        self.result_queue = result_queue
        self.limit = limit

    def run(self):
        try:
            result = self.engine.play(self.board, self.limit)
            self.result_queue.put(result)
        except Exception as e:
            self.result_queue.put(e)

def main():
    args = parse_args()
    script_dir = os.path.dirname(os.path.abspath(__file__))
    engine_path = os.path.abspath(args.engine) if args.engine else os.path.normpath(os.path.join(script_dir, DEFAULT_ENGINE_PATH))
    
    pygame.init()
    screen = pygame.display.set_mode((WINDOW_WIDTH, HEIGHT))
    pygame.display.set_caption(f"Aya Chess Engine - GUI ({args.mode})")
    clock = pygame.time.Clock()
    
    font = pygame.font.SysFont("Arial", 24, bold=True)
    small_font = pygame.font.SysFont("Arial", 18)
    
    load_images()
    board = chess.Board()
    
    try:
        if not os.path.exists(engine_path):
            raise FileNotFoundError(f"Engine not found: {engine_path}")
        engine = chess.engine.SimpleEngine.popen_uci(engine_path)
    except Exception as e:
        print(f"Error starting engine: {e}")
        pygame.quit()
        sys.exit(1)

    selected_sq = None
    running = True
    engine_thinking = False
    result_queue = queue.Queue()
    engine_info = {"score": 0, "bestmove": None, "nodes": 0}

    while running:
        # 1. Event Handling (Always responsive)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            if not engine_thinking and args.mode == "human-vs-engine" and board.turn == chess.WHITE and not board.is_game_over():
                if event.type == pygame.MOUSEBUTTONDOWN:
                    location = pygame.mouse.get_pos()
                    if location[0] < WIDTH:
                        col, row = location[0] // SQ_SIZE, 7 - (location[1] // SQ_SIZE)
                        sq = chess.square(col, row)

                        if selected_sq == sq:
                            selected_sq = None
                        elif selected_sq is not None:
                            move = chess.Move(selected_sq, sq)
                            if board.piece_at(selected_sq) and board.piece_at(selected_sq).piece_type == chess.PAWN and row == 7:
                                move = chess.Move(selected_sq, sq, promotion=chess.QUEEN)

                            if move in board.legal_moves:
                                board.push(move)
                                selected_sq = None
                                # Start analysis for the new position in background
                                engine_thinking = True
                                EngineThread(engine, board, result_queue, chess.engine.Limit(depth=4)).start()
                            else:
                                p = board.piece_at(sq)
                                selected_sq = sq if p and p.color == chess.WHITE else None
                        else:
                            p = board.piece_at(sq)
                            if p and p.color == chess.WHITE:
                                selected_sq = sq

        # 2. Check for Engine Results
        if engine_thinking:
            try:
                result = result_queue.get_nowait()
                engine_thinking = False
                
                if isinstance(result, Exception):
                    print(f"Engine Error: {result}")
                else:
                    if result.info:
                        info = result.info
                        if "score" in info:
                            engine_info["score"] = info["score"].white().score(mate_score=100000)
                        if "nodes" in info:
                            engine_info["nodes"] = info["nodes"]
                        if "pv" in info:
                            engine_info["bestmove"] = info["pv"][0].uci()
                        elif result.move:
                            engine_info["bestmove"] = result.move.uci()
                    
                    # If it was the engine's turn to play, push the move
                    if board.turn == chess.BLACK or args.mode == "engine-vs-engine":
                        if result.move:
                            board.push(result.move)
            except queue.Empty:
                pass

        # 3. Start Engine Turn if needed
        if not board.is_game_over() and not engine_thinking:
            is_engine_turn = (args.mode == "engine-vs-engine") or (board.turn == chess.BLACK)
            if is_engine_turn:
                engine_thinking = True
                # Delay for visual flow (handled inside the loop to stay responsive)
                threading.Timer(0.2, lambda: EngineThread(engine, board, result_queue, chess.engine.Limit(depth=4)).start()).start()

        # 4. Drawing
        draw_board(screen)
        draw_highlights(screen, board, selected_sq)
        draw_pieces(screen, board)
        draw_sidebar(screen, font, small_font, board, engine_info)
        if board.is_game_over():
            draw_game_over(screen, board, WIDTH, HEIGHT)
        
        pygame.display.flip()
        clock.tick(60) # Maintain 60 FPS for smooth UI

    engine.quit()
    pygame.quit()

if __name__ == "__main__":
    main()
