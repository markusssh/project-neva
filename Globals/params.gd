extends Node

const CANV_W = 826
const CANV_H = 648

var drawing_theme_repo: Array[DrawingTheme] = \
[
	DrawingTheme.new("мона лиза", preload("res://Assets/Themes/mona-lisa.png")),
	DrawingTheme.new("крик", preload("res://Assets/Themes/scream.jpg")),
	DrawingTheme.new("неизвестная", preload("res://Assets/Themes/unknown.jpg"))
]
var drawing_theme: DrawingTheme
var rounds: int = 3
var round_time_sec: int = 60

func set_random_theme() -> int:
	drawing_theme = drawing_theme_repo.pick_random()
	return drawing_theme_repo.find(drawing_theme)

func set_theme_by_idx(idx: int) -> void:
	drawing_theme = drawing_theme_repo[idx]
