extends Timer

var round_time: int

#func _ready() -> void:
	#set_process(false)
	#MultiplayerController.RoundStarted.connect(_on_round_started)
	#MultiplayerController.ProcessEventQueue()
#
#func _on_round_started():
	#round_time = MultiplayerController.RoundLength
	#start(round_time)
	#set_process(true)
#
#func _process(delta: float) -> void:
	#%TimeoutProgress.value = abs(time_left - round_time) / round_time * 100
