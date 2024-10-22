extends Node

const CANV_W = 826
const CANV_H = 648
const DEBUG_CODE = "XDEBUG"

var drawing_theme_repo: Array[DrawingTheme] = \
[
	DrawingTheme.new("мона лиза", preload("res://Assets/Themes/mona-lisa.png")),
	DrawingTheme.new("крик", preload("res://Assets/Themes/scream.jpg")),
	DrawingTheme.new("неизвестная", preload("res://Assets/Themes/unknown.jpg"))
]
var rounds: int = 3
