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
	var s1 := StyleBoxFlat.new()
	var outline_value: float = (c.r + c.g + c.b) / 3.0
	s1.bg_color = c
	s1.border_color = Color.BLACK if outline_value > 0.5 else Color.WHITE
	s1.set_border_width_all(5)
	add_theme_stylebox_override("focus", s1)

func _on_toggled(toggled_on: bool) -> void:
	if toggled_on:
		color_picked.emit(color)
