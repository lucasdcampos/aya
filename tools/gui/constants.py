import pygame
import os
import sys

# OS Detection and Engine Path
# The engine executable is usually in src/Aya.CLI/bin/Debug/net9.0/
# On Windows, it is Aya.CLI.exe. On Linux, it is simply Aya.CLI (or Aya.CLI.dll via dotnet)
if sys.platform == "win32":
    DEFAULT_ENGINE_PATH = "../../src/Aya.CLI/bin/Debug/net9.0/Aya.CLI.exe"
else:
    # On Linux, assuming the user might run the published binary or the dll via dotnet
    # For simplicity, we'll check for the file without .exe extension
    DEFAULT_ENGINE_PATH = "../../src/Aya.CLI/bin/Debug/net9.0/Aya.CLI"

# Dimensions
WIDTH, HEIGHT = 600, 600
SIDEBAR_WIDTH = 300
WINDOW_WIDTH = WIDTH + SIDEBAR_WIDTH
SQ_SIZE = WIDTH // 8

# Board Colors
COLORS = [pygame.Color("#eeeed2"), pygame.Color("#769656")]
HIGHLIGHT_COLOR = (186, 202, 43, 150) # Yellowish for selection
LAST_MOVE_COLOR = (155, 199, 0, 100) # Greenish for last move
MOVE_DOT_COLOR = (0, 0, 0, 30) # Subtle dot for legal moves

# UI Colors
SIDEBAR_COLOR = pygame.Color("#262421")
TEXT_COLOR = pygame.Color("#ffffff")
SUB_TEXT_COLOR = pygame.Color("#ababab")
BUTTON_COLOR = pygame.Color("#45423e")
BUTTON_HOVER_COLOR = pygame.Color("#55524e")
