class_name GameActionWebBus extends Resource

var actions: Array[GameAction] = []

func add_message_action(message: String) -> void:
	actions.append(GameAction.new(WebManager.room.my_name, GameAction.ActionType.MESSAGE, [message]))

func add_image_action(image: Image) -> void:
	actions.append(GameAction.new(WebManager.room.my_name, GameAction.ActionType.IMAGE, image.get_data()))
