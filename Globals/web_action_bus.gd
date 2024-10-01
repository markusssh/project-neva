class_name WebActionBus extends Resource

var messages: Array[String] :
	set(value):
		messages = value
		_not_null = true
var image: Image = null :
	set(value):
		image = value
		_not_null = true

var _not_null = false

func add_mesage(m: String) -> void:
	messages.append(m)

func set_image(i: Image) -> void:
	image = i

func pack() -> String:
	var actions: Array[Dictionary] = []
	for m in messages:
		actions.append({"type": "message", "content": m})
	if image != null:
		actions.append({"type": "image", "content": str(image.get_data())})
	return JSONActionWrapper.wrap_actions(actions)
