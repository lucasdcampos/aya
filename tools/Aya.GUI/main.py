import pygame
import chess
import chess.engine
import os
import sys

# Configuration
ENGINE_PATH = os.path.abspath("../Aya.CLI/bin/Debug/net9.0/Aya.CLI.exe")
WIDTH, HEIGHT = 600, 600
SQ_SIZE = WIDTH // 8
COLORS = [pygame.Color("#eeeed2"), pygame.Color("#769656")]
HIGHLIGHT_COLOR = (186, 202, 43, 150) # Yellowish for selection
LAST_MOVE_COLOR = (155, 199, 0, 100) # Greenish for last move
MOVE_DOT_COLOR = (0, 0, 0, 30) # Subtle dot for legal moves

# Piece Image Mapping
PIECE_IMAGES = {}

def load_images():
    pieces = {
        'P': 'white-pawn.png', 'N': 'white-knight.png', 'B': 'white-bishop.png',
        'R': 'white-rook.png', 'Q': 'white-queen.png', 'K': 'white-king.png',
        'p': 'black-pawn.png', 'n': 'black-knight.png', 'b': 'black-bishop.png',
        'r': 'black-rook.png', 'q': 'black-queen.png', 'k': 'black-king.png'
    }
    for symbol, filename in pieces.items():
        path = os.path.join("assets", filename)
        img = pygame.image.load(path)
        PIECE_IMAGES[symbol] = pygame.transform.smoothscale(img, (SQ_SIZE, SQ_SIZE))

def draw_board(screen):
    for r in range(8):
        for c in range(8):
            color = COLORS[(r + c) % 2]
            pygame.draw.rect(screen, color, pygame.Rect(c * SQ_SIZE, r * SQ_SIZE, SQ_SIZE, SQ_SIZE))

def draw_highlights(screen, board, selected_sq):
    # 1. Highlight Last Move
    if board.move_stack:
        last_move = board.peek()
        for sq in [last_move.from_square, last_move.to_square]:
            r, c = 7 - (sq // 8), sq % 8
            s = pygame.Surface((SQ_SIZE, SQ_SIZE), pygame.SRCALPHA)
            s.fill(LAST_MOVE_COLOR)
            screen.blit(s, (c * SQ_SIZE, r * SQ_SIZE))

    # 2. Highlight Selected Square and Legal Moves
    if selected_sq is not None:
        # Selected square highlight
        r, c = 7 - (selected_sq // 8), selected_sq % 8
        s = pygame.Surface((SQ_SIZE, SQ_SIZE), pygame.SRCALPHA)
        s.fill(HIGHLIGHT_COLOR)
        screen.blit(s, (c * SQ_SIZE, r * SQ_SIZE))

        # Legal moves dots
        for move in board.legal_moves:
            if move.from_square == selected_sq:
                target_sq = move.to_square
                tr, tc = 7 - (target_sq // 8), target_sq % 8
                
                # Draw a small circle for empty squares, or a larger ring for captures
                center = (tc * SQ_SIZE + SQ_SIZE // 2, tr * SQ_SIZE + SQ_SIZE // 2)
                if board.piece_at(target_sq):
                    pygame.draw.circle(screen, MOVE_DOT_COLOR, center, SQ_SIZE // 2, 5)
                else:
                    pygame.draw.circle(screen, MOVE_DOT_COLOR, center, SQ_SIZE // 6)

def draw_pieces(screen, board):
    for r in range(8):
        for c in range(8):
            piece = board.piece_at(chess.square(c, 7 - r))
            if piece:
                img = PIECE_IMAGES[piece.symbol()]
                screen.blit(img, pygame.Rect(c * SQ_SIZE, r * SQ_SIZE, SQ_SIZE, SQ_SIZE))

def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Aya Chess Engine - GUI")
    
    load_images()
    
    board = chess.Board()
    
    try:
        engine = chess.engine.SimpleEngine.popen_uci(ENGINE_PATH)
    except Exception as e:
        print(f"Error starting engine: {e}")
        sys.exit(1)

    selected_sq = None
    running = True

    while not board.is_game_over() and running:
        if board.turn == chess.BLACK:
            pygame.time.delay(200)
            result = engine.play(board, chess.engine.Limit(depth=4))
            board.push(result.move)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            
            if event.type == pygame.MOUSEBUTTONDOWN and board.turn == chess.WHITE:
                location = pygame.mouse.get_pos()
                col = location[0] // SQ_SIZE
                row = 7 - (location[1] // SQ_SIZE)
                sq = chess.square(col, row)

                if selected_sq == sq:
                    selected_sq = None
                else:
                    if selected_sq is not None:
                        move = chess.Move(selected_sq, sq)
                        if board.piece_at(selected_sq) and board.piece_at(selected_sq).piece_type == chess.PAWN:
                            if row == 7: move = chess.Move(selected_sq, sq, promotion=chess.QUEEN)
                        
                        if move in board.legal_moves:
                            board.push(move)
                            selected_sq = None
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

        # Drawing sequence
        draw_board(screen)
        draw_highlights(screen, board, selected_sq)
        draw_pieces(screen, board)
        pygame.display.flip()

    engine.quit()
    
    if board.is_game_over():
        print(f"Game Over: {board.result()}")
        while running:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
    
    pygame.quit()

if __name__ == "__main__":
    main()
