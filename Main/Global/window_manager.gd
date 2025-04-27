extends Node

func _ready():
	if not OS.has_feature("debug"):
		return
	# Получаем аргументы командной строки
	var layout = null
	for arg in OS.get_cmdline_args():
		# Предполагаем формат --layout=значение
		if arg.begins_with("layout"):
			var parts = arg.split("=")
			if parts.size() > 1:
				layout = parts[1]
	# Если нет аргумента layout или layout = "fullscreen", делаем полноэкран
	if layout == null or layout == "fullscreen":
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
		return
	# Получаем размер текущего экрана
	var screen_id = DisplayServer.window_get_current_screen(0)
	var screen_size = DisplayServer.screen_get_size(screen_id)
	# Позиционирование на основе layout
	var half_w = screen_size.x / 2
	var half_h = screen_size.y / 2
	match layout:
		"top_left":
			DisplayServer.window_set_position(Vector2i(0, 0))
			DisplayServer.window_set_size(Vector2i(half_w, half_h))
		"top_right":
			DisplayServer.window_set_position(Vector2i(half_w, 0))
			DisplayServer.window_set_size(Vector2i(half_w, half_h))
		"bottom_left":
			DisplayServer.window_set_position(Vector2i(0, half_h))
			DisplayServer.window_set_size(Vector2i(half_w, half_h))
		"bottom_right":
			DisplayServer.window_set_position(Vector2i(half_w, half_h))
			DisplayServer.window_set_size(Vector2i(half_w, half_h))
		"top":
			DisplayServer.window_set_position(Vector2i(0, 0))
			DisplayServer.window_set_size(Vector2i(screen_size.x, half_h))
		"bottom":
			DisplayServer.window_set_position(Vector2i(0, half_h))
			DisplayServer.window_set_size(Vector2i(screen_size.x, half_h))
		"left":
			DisplayServer.window_set_position(Vector2i(0, 0))
			DisplayServer.window_set_size(Vector2i(half_w, screen_size.y))
		"right":
			DisplayServer.window_set_position(Vector2i(half_w, 0))
			DisplayServer.window_set_size(Vector2i(half_w, screen_size.y))
		_:
			# Для любых других значений — полный экран
			DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
