import pygame
import chess
import os
from constants import COLORS, SQ_SIZE, LAST_MOVE_COLOR, HIGHLIGHT_COLOR, MOVE_DOT_COLOR

# Piece Image Mapping
PIECE_IMAGES = {}

def load_images():
    pieces = {
        'P': 'white-pawn.png', 'N': 'white-knight.png', 'B': 'white-bishop.png',
        'R': 'white-rook.png', 'Q': 'white-queen.png', 'K': 'white-king.png',
        'p': 'black-pawn.png', 'n': 'black-knight.png', 'b': 'black-bishop.png',
        'r': 'black-rook.png', 'q': 'black-queen.png', 'k': 'black-king.png'
    }
    script_dir = os.path.dirname(os.path.abspath(__file__))
    for symbol, filename in pieces.items():
        path = os.path.join(script_dir, "assets", filename)
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
        r, c = 7 - (selected_sq // 8), selected_sq % 8
        s = pygame.Surface((SQ_SIZE, SQ_SIZE), pygame.SRCALPHA)
        s.fill(HIGHLIGHT_COLOR)
        screen.blit(s, (c * SQ_SIZE, r * SQ_SIZE))

        for move in board.legal_moves:
            if move.from_square == selected_sq:
                target_sq = move.to_square
                tr, tc = 7 - (target_sq // 8), target_sq % 8
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
