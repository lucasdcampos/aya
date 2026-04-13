import pygame
import chess
import chess.engine
import os
import sys
import argparse

from constants import DEFAULT_ENGINE_PATH, WIDTH, HEIGHT, WINDOW_WIDTH, SQ_SIZE
from renderer import load_images, draw_board, draw_highlights, draw_pieces
from ui import draw_sidebar, draw_game_over

def parse_args():
    parser = argparse.ArgumentParser(description="Aya Chess Engine GUI")
    parser.add_argument(
        "--engine", 
        type=str, 
        default=None, 
        help="Path to the UCI engine executable"
    )
    parser.add_argument(
        "--mode",
        choices=["human-vs-engine", "engine-vs-engine"],
        default="human-vs-engine",
        help="Select game mode: human-vs-engine (default) or engine-vs-engine"
    )
    return parser.parse_args()

def main():
    args = parse_args()
    
    # 1. Resolve Engine Path
    script_dir = os.path.dirname(os.path.abspath(__file__))
    if args.engine:
        engine_path = os.path.abspath(args.engine)
    else:
        engine_path = os.path.normpath(os.path.join(script_dir, DEFAULT_ENGINE_PATH))
    
    pygame.init()
    screen = pygame.display.set_mode((WINDOW_WIDTH, HEIGHT))
    pygame.display.set_caption(f"Aya Chess Engine - GUI ({args.mode})")
    
    font = pygame.font.SysFont("Arial", 24, bold=True)
    small_font = pygame.font.SysFont("Arial", 18)
    
    load_images()
    board = chess.Board()
    
    try:
        if not os.path.exists(engine_path):
            raise FileNotFoundError(f"Engine executable not found at: {engine_path}")
        engine = chess.engine.SimpleEngine.popen_uci(engine_path)
    except Exception as e:
        print(f"Error starting engine: {e}")
        pygame.quit()
        sys.exit(1)

    selected_sq = None
    running = True
    engine_info = {
        "score": 0,
        "bestmove": None,
        "nodes": 0
    }

    while running:
        # 1. DRAWING FIRST (Ensure UI reflects latest board state immediately)
        draw_board(screen)
        draw_highlights(screen, board, selected_sq)
        draw_pieces(screen, board)
        draw_sidebar(screen, font, small_font, board, engine_info)
        
        if board.is_game_over():
            draw_game_over(screen, board, WIDTH, HEIGHT)
        
        pygame.display.flip()

        # 2. Event Handling
        human_moved = False
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            if args.mode == "human-vs-engine" and board.turn == chess.WHITE and not board.is_game_over():
                if event.type == pygame.MOUSEBUTTONDOWN:
                    location = pygame.mouse.get_pos()
                    if location[0] < WIDTH:
                        col = location[0] // SQ_SIZE
                        row = 7 - (location[1] // SQ_SIZE)
                        sq = chess.square(col, row)

                        if selected_sq == sq:
                            selected_sq = None
                        elif selected_sq is not None:
                            move = None
                            if board.piece_at(selected_sq) and board.piece_at(selected_sq).piece_type == chess.PAWN:
                                if row == 7:
                                    move = chess.Move(selected_sq, sq, promotion=chess.QUEEN)

                            if move is None:
                                move = chess.Move(selected_sq, sq)

                            if move in board.legal_moves:
                                board.push(move)
                                selected_sq = None
                                human_moved = True
                            else:
                                target_piece = board.piece_at(sq)
                                if target_piece and target_piece.color == chess.WHITE:
                                    selected_sq = sq
                                else:
                                    selected_sq = None
                        else:
                            piece = board.piece_at(sq)
                            if piece and piece.color == chess.WHITE:
                                selected_sq = sq

        if not running: break

        # If human moved, we redraw ONCE more to show the piece at target before engine starts
        if human_moved:
            # Immediate feedback redraw
            draw_board(screen)
            draw_highlights(screen, board, selected_sq)
            draw_pieces(screen, board)
            draw_sidebar(screen, font, small_font, board, engine_info)
            pygame.display.flip()

            # Sync update stats (briefly blocks, but AFTER move is visible)
            info = engine.analyse(board, chess.engine.Limit(depth=4))
            if "score" in info:
                engine_info["score"] = info["score"].white().score(mate_score=100000)
            if "nodes" in info:
                engine_info["nodes"] = info["nodes"]
            if "pv" in info:
                engine_info["bestmove"] = info["pv"][0].uci()

        # 3. Game Logic & Engine Turn
        if not board.is_game_over():
            is_engine_turn = False
            if args.mode == "engine-vs-engine":
                is_engine_turn = True
            elif args.mode == "human-vs-engine" and board.turn == chess.BLACK:
                is_engine_turn = True

            if is_engine_turn:
                # Small delay so move is visible
                pygame.time.delay(200)

                # Play with a limit and capture info
                result = engine.play(board, chess.engine.Limit(depth=4))

                if result.info:
                    info = result.info
                    if "score" in info:
                        # Normalize to White perspective: positive = white winning
                        engine_info["score"] = info["score"].white().score(mate_score=100000)
                    if "nodes" in info:
                        engine_info["nodes"] = info["nodes"]
                    if "pv" in info:
                        engine_info["bestmove"] = info["pv"][0].uci()
                    elif result.move:
                        engine_info["bestmove"] = result.move.uci()

                if result.move:
                    board.push(result.move)

    engine.quit()
    pygame.quit()

if __name__ == "__main__":
    main()
