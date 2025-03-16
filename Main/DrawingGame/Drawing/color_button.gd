class_name ColorButton extends Button

var color: Color

static func create(c: Color, num: int, group: ButtonGroup) -> ColorButton:
	var b: ColorButton = ColorButton.new()
	b.custom_minimum_size = Vector2(100, 100)
	
	b.color = c
	
	var s := StyleBoxFlat.new()
	s.bg_color = c
	b.add_theme_stylebox_override("hover", s)
	b.add_theme_stylebox_override("pressed", s)
	b.add_theme_stylebox_override("normal", s)
	
	var s1 := StyleBoxFlat.new()
	var outline_value: float = (c.r + c.g + c.b) / 3.0
	s1.bg_color = c
	s1.border_color = Color.BLACK if outline_value > 0.5 else Color.WHITE
	s1.set_border_width_all(5)
	b.add_theme_stylebox_override("focus", s1)
	
	if num <= 10:
		b.shortcut = Shortcut.new()
		var keycode: int
		if num == 10:
			keycode = 48
		else:
			keycode = 48 + num
		var i: InputEventKey = InputEventKey.new()
		i.physical_keycode = keycode
		i.unicode = keycode
		b.shortcut.events.append(i)
	
	b.button_group = group
	b.toggle_mode = true
	
	return b
