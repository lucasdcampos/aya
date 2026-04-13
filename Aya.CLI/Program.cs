using Aya;

// The main entry point for the Aya Chess Engine.
// It starts the UCI (Universal Chess Interface) handler to communicate with chess GUIs.
var uci = new UciHandler();
uci.Run();
