extends Control

signal drawing_baked(i: Image)

enum PaintingMode {
	PIPETTE,
	BRUSH,
	ERASER,
	BUCKET,
	LINE,
	EMPTY_RECTANGLE,
	FULL_RECTANGLE,
	EMPTY_CIRCLE,
	FULL_CIRCLE
}

enum BrushSize {
	XS,
	S,
	M,
	L,
	XL
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

@export var drawing_line: Line2D
@export var paint_viewport: SubViewport
@export var painted_image: TextureRect
@export var background: ColorRect
@export var erase_mask_line: Line2D
@export var erase_mask_viewport: SubViewport
@export var color_picker: ColorPicker
@export var color_button: ColorButton

var e: float = 1
var mouse_pos: Vector2
var mode: PaintingMode = PaintingMode.BRUSH 
var canv_size: Vector2i
var history_scroll_idx: int = 1
var drawing_history: Array[ImageTexture] = [ImageTexture.new()]
#TODO: can be loaded from
#1. presets
#2. most common color from chosen art
var pallete: Array[Color] = [
	Color.DARK_SLATE_GRAY,
	Color.BROWN,
	Color.SEA_GREEN,
	Color.CADET_BLUE,
	Color.INDIAN_RED,
	Color.YELLOW_GREEN,
	Color.PLUM,
	Color.NAVY_BLUE
]

var brush_size: BrushSize = DEFAULT_SIZE:
	set(value):
		brush_size = value
		drawing_line.width = BRUSH_SIZE_DICT[value]
		erase_mask_line.width =  BRUSH_SIZE_DICT[value]
var drawing_color: Color = pallete[0]: 
	set(value):
		drawing_color = value
		color_picker.color = value

func _ready() -> void:
	color_picker.color = drawing_color
	drawing_line.width = BRUSH_SIZE_DICT[brush_size]
	drawing_line.width = BRUSH_SIZE_DICT[brush_size]
	_set_palette_buttons()

func allign_canvas() -> void:
	for child in get_children():
		child.size = canv_size
		if child is SubViewportContainer:
			child.get_child(0).size = canv_size
	background.pivot_offset = canv_size / 2

func _process(_delta: float) -> void:
	if Input.is_action_just_released("step_forward"):
		_history_step_forward()
	elif Input.is_action_just_released("step_back"):
		_history_step_back()
	match mode:
		PaintingMode.PIPETTE:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_pipette_pick()
		PaintingMode.BRUSH:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_brush_drawing()
		PaintingMode.ERASER:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				set_drawing_canvas_mouse_pos()
				handle_erase()

func _input(event: InputEvent) -> void:
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

func set_drawing_canvas_mouse_pos() -> void:
	mouse_pos = paint_viewport.get_mouse_position()

func inside_drawing_canvas() -> bool:
	return mouse_pos.x < paint_viewport.size.x and mouse_pos.y < paint_viewport.size.y and mouse_pos.x >= 0 and mouse_pos.y >= 0

func handle_pipette_pick() -> void:
	if inside_drawing_canvas():
		var p: Color = paint_viewport.get_texture().get_image().get_pixelv(mouse_pos)
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
	erase_mask_line.width = drawing_line.width
	erase_mask_line.points = drawing_line.points

#Viewport texture can't be written and read at the same time by engine
#So we should convert to image and back to texture
#Doesn't even work with 'call_deferred()' method
func bake_drawing() -> void:
	var image: Image = _get_baked_image()
	var viewport_image: Image = _get_viewport_image()
	if image == null:
				image = viewport_image
	match mode:
		PaintingMode.ERASER:
			await RenderingServer.frame_post_draw
			var erase_mask: Image = erase_mask_viewport.get_texture().get_image()
			for row in canv_size.y:
				for column in canv_size.x:
					if erase_mask.get_pixel(column, row) == Color.BLACK:
						image.set_pixel(column, row, Color(0, 0, 0, 0))
			painted_image.texture = ImageTexture.create_from_image(image)
			drawing_line.clear_points()
		PaintingMode.BUCKET:
			var replace_color = image.get_pixelv(mouse_pos)
			painted_image.texture = ImageTexture.create_from_image(
				AlgoUtil.FloodFill(image, mouse_pos, replace_color, drawing_color))
		_:
			painted_image.texture = ImageTexture.create_from_image(viewport_image)
			drawing_line.clear_points()
	_update_history()
	drawing_baked.emit(_get_baked_image())

func _get_baked_image() -> Image:
	return painted_image.texture.get_image()

func _get_viewport_image() -> Image:
	return paint_viewport.get_texture().get_image()

func _update_history() -> void:
	if history_scroll_idx > 1:
		drawing_history = drawing_history.slice(0, -history_scroll_idx + 1)
		history_scroll_idx = 1
	if drawing_history.size() >= HISTORY_MAX_SIZE:
		drawing_history = drawing_history.slice(1, drawing_history.size())
	drawing_history.append(painted_image.texture)

func _history_step_back() -> void:
	if history_scroll_idx + 1 <= drawing_history.size():
		history_scroll_idx += 1
		painted_image.texture = drawing_history[-history_scroll_idx]

func _history_step_forward() -> void:
	if history_scroll_idx >= 2:
		history_scroll_idx -= 1
		painted_image.texture = drawing_history[-history_scroll_idx]

#region COLOR PICKER UI
func _on_color_picker_color_changed(color: Color) -> void:
	if drawing_line.points.is_empty():
		drawing_color = color
#endregion

#region INSTRUMENT BUTTON GROUP
#ALERT: rework procedural gen
func _on_pipette_buttton_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.PIPETTE

func _on_brush_buttton_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BRUSH

func _on_eraser_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.ERASER

func _on_bucket_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BUCKET

func _on_line_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.LINE

func _on_empty_rectangle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.EMPTY_RECTANGLE

func _on_full_rectangle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.FULL_RECTANGLE

func _on_empty_circle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.EMPTY_CIRCLE

func _on_full_circle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.FULL_CIRCLE
#endregion

#region HISTORY BUTTONS
func _on_action_back_button_pressed() -> void:
	_history_step_back()

func _on_action_forward_button_pressed() -> void:
	_history_step_forward()
#endregion

#region BRUSH SIZE BUTTONS
#ALERT: rework procedural gen
func _on_xs_button_pressed() -> void:
	brush_size = BrushSize.XS

func _on_s_button_pressed() -> void:
	brush_size = BrushSize.S

func _on_m_button_pressed() -> void:
	brush_size = BrushSize.M

func _on_l_button_pressed() -> void:
	brush_size = BrushSize.L

func _on_xl_button_pressed() -> void:
	brush_size = BrushSize.XL
#endregion

func _set_palette_buttons() -> void:
	for color in pallete:
		var n: ColorButton = color_button.duplicate()
		%PalleteHFlowContainer.add_child(n)
		n.show()
		n.set_color(color)
		n.connect("color_picked", _on_color_picked)

func _on_color_picked(c: Color) -> void:
	drawing_color = c
