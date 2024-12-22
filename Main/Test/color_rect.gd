extends TextureRect

var mat = material as ShaderMaterial

func _ready() -> void:
	mat.set_shader_parameter("object_origin", position)
