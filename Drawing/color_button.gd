class_name ColorButton extends Button

signal color_picked(c: Color)

var color: Color

func set_color(c: Color) -> void:
	color = c
	var s := StyleBoxFlat.new()
	s.bg_color = c
	add_theme_stylebox_override("hover", s)
	add_theme_stylebox_override("pressed", s)
	add_theme_stylebox_override("normal", s)

func _on_toggled(toggled_on: bool) -> void:
	if toggled_on:
		color_picked.emit(color)
