class_name DrawingManager
extends Node

enum PaintingMode {
	PICKER,
	BRUSH,
	ERASER,
	BUCKET
}

enum BrushSize {
	XS = 1,
	S = 2,
	M = 3,
	L = 4,
	XL = 5
}

const DEFAULT_SIZE: BrushSize = BrushSize.M
const HISTORY_MAX_SIZE: int = 40
const BRUSH_SIZE_DICT := {
	BrushSize.XS: 15,
	BrushSize.S: 30,
	BrushSize.M: 40,
	BrushSize.L: 80,
	BrushSize.XL: 120
}

@export_category("Drawing Canvas")
@export var drawing_line: Line2D
@export var canvas_viewport: SubViewport
@export var drawing_image: TextureRect
@export var background: ColorRect
@export var brush_circle: BrushCircle

@export_category("Colors")
@export var color_picker: ColorPicker
@export var pallete_button_group: ButtonGroup
@export var picked_color: ColorRect
@export var pallete_container: GridContainer

@export_category("Instruments")
@export var brush: TextureButton

var drawing_on: bool = true
var e: float = 1
var mouse_pos: Vector2
var mode: PaintingMode = PaintingMode.BRUSH:
	set(value):
		mode = value
		match value:
			PaintingMode.BRUSH:
				brush_circle.show()
			PaintingMode.ERASER:
				brush_circle.show()
			_:
				brush_circle.hide()
var canv_size: Vector2i
var history_scroll_idx: int = 1
var drawing_history: Array[ImageTexture]

# TODO: сделать пресеты
var pallete: Array[Color] = [
	Color.DARK_SLATE_GRAY,
	Color.BROWN,
	Color.SEA_GREEN,
	Color.CADET_BLUE,
	Color.INDIAN_RED,
	Color.YELLOW_GREEN,
	Color.PLUM,
	Color.NAVY_BLUE,
	Color.BLACK,
	Color.WHITE
]

var brush_size: BrushSize = BrushSize.M:
	set(value):
		brush_size = value
		var pixel_size = BRUSH_SIZE_DICT[value]
		brush_circle.set_circle_size(pixel_size)
		drawing_line.width = pixel_size
var drawing_color: Color = pallete[0]: 
	set(value):
		drawing_color = value
		color_picker.color = value
		picked_color.color = value

func _ready() -> void:
	brush_size = DEFAULT_SIZE
	color_picker.color = drawing_color
	picked_color.color = drawing_color
	drawing_line.width = BRUSH_SIZE_DICT[brush_size]
	
	var initial_image = ImageHelper.CreateEmptyImage()
	for y in range(initial_image.get_height()):
		for x in range(initial_image.get_width()):
			initial_image.set_pixel(x, y, background.color)
	drawing_image.texture = ImageTexture.create_from_image(initial_image)
	drawing_history.append(drawing_image.texture)
	
	set_palette_buttons()

func allign_canvas() -> void:
	for child in get_children():
		child.size = canv_size
		if child is SubViewportContainer:
			child.get_child(0).size = canv_size
	background.pivot_offset = canv_size / 2

func set_palette_buttons() -> void:
	var i: int = 1
	for color in pallete:
		var n: ColorButton = ColorButton.create(color, i, pallete_button_group)
		pallete_container.add_child(n)
		n.pressed.connect(_on_color_button_pressed.bind(n.color))
		i += 1

func _process(_delta: float) -> void:
	if not drawing_on:
		return
	
	match mode:
		PaintingMode.PICKER:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_picker_pick()
		PaintingMode.BRUSH:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_brush_drawing()
		PaintingMode.ERASER:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_erase()

func _input(event: InputEvent) -> void:
	if not drawing_on:
		return
	
	match mode:
		PaintingMode.BRUSH:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					if drawing_line.points.size() > 0:
						bake_drawing()
		PaintingMode.ERASER:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					if drawing_line.points.size() > 0:
						bake_drawing()
		PaintingMode.BUCKET:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					set_drawing_canvas_mouse_pos()
					if inside_drawing_canvas():
						bake_drawing()
		PaintingMode.PICKER:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					brush.button_pressed = true

func set_drawing_canvas_mouse_pos() -> void:
	mouse_pos = canvas_viewport.get_mouse_position()

func inside_drawing_canvas() -> bool:
	return mouse_pos.x < canvas_viewport.size.x \
	and mouse_pos.y < canvas_viewport.size.y \
	and mouse_pos.x >= 0 \
	and mouse_pos.y >= 0

func handle_picker_pick() -> void:
	if inside_drawing_canvas():
		var p: Color = canvas_viewport.get_texture().get_image().get_pixelv(mouse_pos)
		if p.a != 0:
			drawing_color = p
		else: drawing_color = background.color

func handle_brush_drawing() -> void:
	match mode:
		PaintingMode.BRUSH:
			drawing_line.default_color = drawing_color
		PaintingMode.ERASER:
			drawing_line.default_color = background.color
	if drawing_line.points.size() == 0:
		if inside_drawing_canvas():
			drawing_line.add_point(mouse_pos)
			#Adding second point do make visible dot
			drawing_line.add_point(mouse_pos + Vector2.DOWN)
	elif absf(mouse_pos.x - drawing_line.points[-1].x) >= e \
		or absf(mouse_pos.y - drawing_line.points[-1].y) >= e:
		drawing_line.add_point(mouse_pos)

func handle_erase() -> void:
	handle_brush_drawing()

# Viewport texture can't be written and read at the same time by engine
# So we should convert to image and back to texture
# Doesn't even work with 'call_deferred()' method
func bake_drawing() -> void:
	var image: Image = get_baked_image()
	var viewport_image: Image = get_viewport_image()
	if image == null:
				image = viewport_image
	match mode:
		PaintingMode.BUCKET:
			var replace_color = image.get_pixelv(mouse_pos)
			drawing_image.texture = ImageTexture.create_from_image(
				AlgoUtil.FloodFill(image, mouse_pos, replace_color, drawing_color))
		_:
			drawing_image.texture = ImageTexture.create_from_image(viewport_image)
			drawing_line.clear_points()
	update_history()

func get_baked_image() -> Image:
	return drawing_image.texture.get_image()

func get_viewport_image() -> Image:
	return canvas_viewport.get_texture().get_image()

func update_history() -> void:
	if history_scroll_idx > 1:
		drawing_history = drawing_history.slice(0, -history_scroll_idx + 1)
		history_scroll_idx = 1
	if drawing_history.size() >= HISTORY_MAX_SIZE:
		drawing_history = drawing_history.slice(1, drawing_history.size())
	drawing_history.append(drawing_image.texture)

func history_step_back() -> void:
	if history_scroll_idx + 1 <= drawing_history.size():
		history_scroll_idx += 1
		var curr_texture = drawing_history[-history_scroll_idx]
		drawing_image.texture = curr_texture

func history_step_forward() -> void:
	if history_scroll_idx >= 2:
		history_scroll_idx -= 1
		var curr_texture = drawing_history[-history_scroll_idx]
		drawing_image.texture = curr_texture

#region COLOR PICKER UI
func _on_color_picker_color_changed(color: Color) -> void:
	if drawing_line.points.is_empty():
		drawing_color = color
#endregion

#region INSTRUMENT BUTTON GROUP
func _on_picker_buttton_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.PICKER

func _on_brush_buttton_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BRUSH

func _on_eraser_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.ERASER

func _on_bucket_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BUCKET
#endregion

#region HISTORY BUTTONS
func _on_action_back_button_pressed() -> void:
	history_step_back()

func _on_action_forward_button_pressed() -> void:
	history_step_forward()
#endregion

#region BRUSH SIZE
func _on_brush_size_slider_value_changed(value: float) -> void:
	@warning_ignore("int_as_enum_without_cast")
	brush_size = int(value)
#endregion

func _on_color_button_pressed(c: Color) -> void:
	drawing_color = c

func save_painting() -> void:
	var i: Image = get_baked_image()
	var save_path := "user://1.png"
	var err = i.save_png(save_path)
	if err != OK:
		Logger.LogMessage("[color=red]Couldn't save the painting to: [i]" + save_path + "[/i][/color]")
	else:
		Logger.LogMessage("Painting was saved to: [i]" + save_path + "[/i]")

func _on_save_button_pressed() -> void:
	save_painting()
