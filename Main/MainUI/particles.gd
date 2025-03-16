extends ColorRect

@export var noise_texture: NoiseTexture2D

func _process(delta: float) -> void:
	noise_texture.noise.offset.z += 10*delta
