extends Control

@export var showcase: GridContainer
@export var curr_drawing: TextureRect
@export var drawing_display: SplitContainer
@export var stars_container: StarsContainer

func _ready() -> void:
	showcase.hide()
	var images: Dictionary
	
	for player in MultiplayerController.CurrentLobbyPlayers:
		images[player] = ImageHelper.CreateImageFromCompressed(
			MultiplayerController.CurrentLobbyPlayers[player].FinalImageData)
	
	showcase.columns = ceili(sqrt(images.size()))
	
	for player in images:
		if Networking.IsMyPeer(player):
			var t: TextureRect = TextureRect.new()
			t.texture = ImageTexture.create_from_image(images[player])
			showcase.add_child(t)
			continue
		
		var timer: Timer = Timer.new()
		timer.one_shot = true
		add_child(timer)
		
		curr_drawing.texture = ImageTexture.create_from_image(images[player])
		
		timer.start(MultiplayerController.TimeToRateOneSec)
		await timer.timeout
		timer.queue_free()
		
		showcase.add_child(curr_drawing.duplicate())
		var score := stars_container.get_score()
		MultiplayerController.SendScoreFromClient(player, score)
		stars_container.clear_score()
	
	drawing_display.hide()
	showcase.show()
