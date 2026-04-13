import pygame
import chess
from constants import WIDTH, HEIGHT, SIDEBAR_WIDTH, SIDEBAR_COLOR, TEXT_COLOR, SUB_TEXT_COLOR

def draw_sidebar(screen, font, small_font, board, engine_info):
    sidebar_rect = pygame.Rect(WIDTH, 0, SIDEBAR_WIDTH, HEIGHT)
    pygame.draw.rect(screen, SIDEBAR_COLOR, sidebar_rect)

    y_offset = 20

    # Title
    title_surface = font.render("Aya Chess Engine", True, TEXT_COLOR)
    screen.blit(title_surface, (WIDTH + 20, y_offset))
    y_offset += 50

    # Status
    turn_text = "White to move" if board.turn == chess.WHITE else "Black to move (Thinking...)"
    status_surface = small_font.render(turn_text, True, SUB_TEXT_COLOR)
    screen.blit(status_surface, (WIDTH + 20, y_offset))
    y_offset += 40

    # Engine Stats
    eval_val = engine_info.get("score")
    if eval_val is not None:
        score_text = f"Eval: {eval_val / 100.0:+.2f}"
    else:
        score_text = "Eval: 0.00"
    
    eval_surface = font.render(score_text, True, TEXT_COLOR)
    screen.blit(eval_surface, (WIDTH + 20, y_offset))
    y_offset += 40

    best_move = engine_info.get("bestmove")
    bm_surface = small_font.render(f"Best Move: {best_move if best_move else '...'}", True, SUB_TEXT_COLOR)
    screen.blit(bm_surface, (WIDTH + 20, y_offset))
    y_offset += 30

    nodes = engine_info.get("nodes")
    nodes_surface = small_font.render(f"Nodes: {nodes if nodes else '0'}", True, SUB_TEXT_COLOR)
    screen.blit(nodes_surface, (WIDTH + 20, y_offset))
    y_offset += 50

    # Game History
    history_title = font.render("Move History", True, TEXT_COLOR)
    screen.blit(history_title, (WIDTH + 20, y_offset))
    y_offset += 40

    moves = board.move_stack
    start_idx = max(0, len(moves) - 10)
    for i in range(start_idx, len(moves)):
        move = moves[i]
        move_num = (i // 2) + 1
        prefix = f"{move_num}. " if i % 2 == 0 else "   "
        color = TEXT_COLOR if i % 2 == 0 else SUB_TEXT_COLOR
        move_surface = small_font.render(f"{prefix}{move.uci()}", True, color)
        
        x_pos = WIDTH + 20 + (140 if i % 2 != 0 else 0)
        row_y = y_offset + ((i - start_idx) // 2) * 25
        screen.blit(move_surface, (x_pos, row_y))

def draw_game_over(screen, board, WIDTH, HEIGHT):
    overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 150))
    screen.blit(overlay, (0, 0))
    
    go_font = pygame.font.SysFont("Arial", 48, bold=True)
    go_surface = go_font.render(f"Game Over: {board.result()}", True, (255, 255, 255))
    screen.blit(go_surface, (WIDTH // 2 - go_surface.get_width() // 2, HEIGHT // 2 - 24))
