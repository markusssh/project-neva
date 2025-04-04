class_name ShowcaseItem extends VBoxContainer

var image: Image
var num: int
var player: String
var score: int

static func create(p_image: Image, p_num: int, p_player: String, p_score: int) -> ShowcaseItem:
	var i := ShowcaseItem.new()
	i.image = p_image
	i.num = p_num
	i.player = p_player
	i.score = p_score
	return i

func _ready() -> void:
	var drawing := TextureRect.new()
	drawing.texture = ImageTexture.create_from_image(image)
	var label: Label = Label.new()
	label.text = str(num) + ". " + player + ": " + str(score)
	label.add_theme_color_override("font_color", Color.BLACK)
	add_child(drawing)
	add_child(label)
