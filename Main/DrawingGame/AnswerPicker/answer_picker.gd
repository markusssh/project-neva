extends Control

@export var content_grid: HFlowContainer

func _ready() -> void:
	open_authors()

func open_authors() -> void:
	clear_content()
	%BackButton.hide()
	for author in Database.get_authors_list():
		var button = Button.new()
		button.pressed.connect(_on_author_button_pressed.bind(author.get("id")))
		button.text = author.get("name")
		content_grid.add_child(button)

func _on_author_button_pressed(author_id: int) -> void:
	open_paintings(author_id)

func open_paintings(author_id: int) -> void:
	clear_content()
	%BackButton.show()
	for painting in Database.get_paintings_list(author_id):
		var button = Button.new()
		button.pressed.connect(_on_painting_button_pressed.bind(painting.get("id")))
		button.text = painting.get("name")
		content_grid.add_child(button)

func _on_painting_button_pressed(painting_id: int) -> void:
	%PickedPaintingLabel.text = "ID of the answer: " + str(painting_id)
	open_authors()

func clear_content() -> void:
	for child in content_grid.get_children():
		child.queue_free()

func _on_back_button_pressed() -> void:
	open_authors()
