class_name ShowcaseItem extends VBoxContainer

@export var top_label: Label
@export var drawing_container: PanelContainer
@export var bottom_label: Label

var image: Image
var num: int
var player: String
var score: int

var drawing_scale = 4.0 / 7.0

static func create(p_image: Image, p_num: int, p_player: String, p_score: int) -> ShowcaseItem:
	var i := ShowcaseItem.new()
	i.image = p_image
	i.num = p_num
	i.player = p_player
	i.score = p_score
	return i

func _ready() -> void:
	top_label.text = str(num) + ". " + player
	bottom_label.text = str(score)
	
	var drawing := TextureRect.new()
	drawing.texture = ImageTexture.create_from_image(image)
	drawing.size = drawing.size * drawing_scale
	drawing.set_anchors_preset(Control.PRESET_CENTER)
	drawing_container.add_child(drawing)
