class_name ServerResponse extends Resource

var from: String
var action: GameAction

func _init(from: String, action: GameAction) -> void:
	self.from = from
	self.action = action
