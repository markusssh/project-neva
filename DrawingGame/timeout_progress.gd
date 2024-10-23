extends ProgressBar

func _process(delta: float) -> void:
	value = abs(%GameModeTimer.time_left - Params.round_time_sec) / Params.round_time_sec * 100
