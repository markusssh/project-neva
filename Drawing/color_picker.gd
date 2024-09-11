extends ColorPicker

func _ready() -> void:
	color = %DrawingLine.default_color

#ALERT: doesn't work. Should freeze ui if not hovered
func _on_mouse_exited() -> void:
	release_focus()
