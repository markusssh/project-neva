class_name Player extends Object

signal score_changed(player: Object)

#TODO: add name validation (only 1 in game)
var name: String = "Player"
var score: int = 0

func _init(p_name: String) -> void:
	name = p_name

func add_score(add: int) -> void:
	score += add
	score_changed.emit(self)
