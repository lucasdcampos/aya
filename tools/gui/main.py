import pygame
import chess
import chess.engine
import chess.pgn
import os
import sys
import argparse
import threading
import queue
import time
import pyperclip
from datetime import datetime
from tkinter import filedialog, Tk

from constants import DEFAULT_ENGINE_PATH, WIDTH, HEIGHT, WINDOW_WIDTH, SQ_SIZE
from renderer import load_images, draw_board, draw_highlights, draw_pieces
from ui import draw_sidebar, draw_game_over

def get_pgn_string(board, white_name, black_name):
    game = chess.pgn.Game.from_board(board)
    game.headers["White"] = white_name
    game.headers["Black"] = black_name
    game.headers["Date"] = datetime.now().strftime("%Y.%m.%d")
    return str(game)

def export_pgn_dialog(board, white_name, black_name):
    pgn_str = get_pgn_string(board, white_name, black_name)
    
    # Hide the main tkinter window
    root = Tk()
    root.withdraw()
    
    filename = filedialog.asksaveasfilename(
        defaultextension=".pgn",
        filetypes=[("PGN files", "*.pgn"), ("All files", "*.*")],
        initialfile=f"game_{datetime.now().strftime('%Y%m%d_%H%M%S')}.pgn",
        title="Export PGN"
    )
    
    if filename:
        with open(filename, "w") as f:
            f.write(pgn_str)
        print(f"PGN exported to {filename}")
    
    root.destroy()

def parse_args():
    parser = argparse.ArgumentParser(description="Aya Chess Engine GUI")
    parser.add_argument("--engine", type=str, default=None, help="Path to the primary UCI engine (Aya)")
    parser.add_argument("--opponent", type=str, default=None, help="Path to the opponent UCI engine (e.g., Stockfish)")
    parser.add_argument("--mode", choices=["human-vs-engine", "engine-vs-engine"], default="human-vs-engine", help="Game mode")
    return parser.parse_args()

class EngineThread(threading.Thread):
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
    opponent_path = os.path.abspath(args.opponent) if args.opponent else None

    pygame.init()
    screen = pygame.display.set_mode((WINDOW_WIDTH, HEIGHT))
    pygame.display.set_caption("Aya Chess Engine - GUI")
    clock = pygame.time.Clock()
    font, small_font = pygame.font.SysFont("Arial", 24, bold=True), pygame.font.SysFont("Arial", 18)
    
    load_images()
    board = chess.Board()
    
    engines = {}
    try:
        engine_white = chess.engine.SimpleEngine.popen_uci(engine_path)
        engines[chess.WHITE] = engine_white
        engines[chess.BLACK] = chess.engine.SimpleEngine.popen_uci(opponent_path) if opponent_path else engine_white
    except Exception as e:
        print(f"Error starting engines: {e}"); pygame.quit(); sys.exit(1)

    selected_sq, running, engine_thinking = None, True, False
    result_queue = queue.Queue()
    engine_info = {
        "score": 0, "bestmove": None, "nodes": 0,
        "white_name": engines[chess.WHITE].id.get("name", "White"),
        "black_name": engines[chess.BLACK].id.get("name", "Black")
    }
    
    feedback_msg = ""
    feedback_timer = 0

    while running:
        # 1. Drawing
        draw_board(screen)
        draw_highlights(screen, board, selected_sq)
        draw_pieces(screen, board)
        
        # Clear feedback after 3 seconds
        if feedback_msg and time.time() > feedback_timer:
            feedback_msg = ""

        copy_btn, export_btn = draw_sidebar(screen, font, small_font, board, engine_info, feedback_msg)
        if board.is_game_over(): draw_game_over(screen, board, WIDTH, HEIGHT)
        pygame.display.flip()

        # 2. Event Handling
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            
            if event.type == pygame.MOUSEBUTTONDOWN:
                mouse_pos = pygame.mouse.get_pos()
                
                # Check Buttons
                if copy_btn.collidepoint(mouse_pos):
                    pgn_str = get_pgn_string(board, engine_info["white_name"], engine_info["black_name"])
                    pyperclip.copy(pgn_str)
                    feedback_msg = "PGN Copied to Clipboard!"
                    feedback_timer = time.time() + 3
                    continue
                
                if export_btn.collidepoint(mouse_pos):
                    export_pgn_dialog(board, engine_info["white_name"], engine_info["black_name"])
                    feedback_msg = "PGN Exported!"
                    feedback_timer = time.time() + 3
                    continue

                if not engine_thinking and args.mode == "human-vs-engine" and board.turn == chess.WHITE and not board.is_game_over():
                    if mouse_pos[0] < WIDTH:
                        col, row = mouse_pos[0] // SQ_SIZE, 7 - (mouse_pos[1] // SQ_SIZE)
                        sq = chess.square(col, row)
                        if selected_sq == sq: selected_sq = None
                        elif selected_sq is not None:
                            move = chess.Move(selected_sq, sq)
                            if board.piece_at(selected_sq) and board.piece_at(selected_sq).piece_type == chess.PAWN and row == 7:
                                move = chess.Move(selected_sq, sq, promotion=chess.QUEEN)
                            if move in board.legal_moves:
                                board.push(move)
                                selected_sq = None
                            else:
                                p = board.piece_at(sq)
                                selected_sq = sq if p and p.color == chess.WHITE else None
                        else:
                            p = board.piece_at(sq)
                            if p and p.color == chess.WHITE: selected_sq = sq

        # 3. Engine Results & Turn logic (Assíncrono)
        if engine_thinking:
            try:
                result = result_queue.get_nowait()
                engine_thinking = False
                if not isinstance(result, Exception):
                    info = result.info
                    if info:
                        if "score" in info: engine_info["score"] = info["score"].white().score(mate_score=100000)
                        if "nodes" in info: engine_info["nodes"] = info["nodes"]
                        if "pv" in info: engine_info["bestmove"] = info["pv"][0].uci()
                    if result.move: board.push(result.move)
            except queue.Empty: pass

        if not board.is_game_over() and not engine_thinking:
            if (args.mode == "engine-vs-engine") or (board.turn == chess.BLACK):
                engine_thinking = True
                current_engine = engines[board.turn]
                threading.Timer(0.2, lambda: EngineThread(current_engine, board, result_queue, chess.engine.Limit(depth=4)).start()).start()

        clock.tick(60)

    for e in set(engines.values()): e.quit()
    pygame.quit()

if __name__ == "__main__":
    main()
