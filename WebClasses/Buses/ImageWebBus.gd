class_name ImageWebBus extends Resource

const PACKET_SIZE = 64000

var image: Image = null

func add_image(image: Image) -> void:
	var adfdsgfdsf = image.get_data()
	self.image = image

#func packetize_image() -> Array[ImagePacket]:
	#var res: Array[ImagePacket] = []
	#var compressed = image.get_data().compress(FileAccess.CompressionMode.COMPRESSION_ZSTD)
	#var total_packets = ceil(float(compressed.size()) / PACKET_SIZE)
	#for pack_num in range(total_packets):
		#var start = pack_num * PACKET_SIZE
		#var end = min((pack_num + 1) * PACKET_SIZE, compressed.size())
		#res.append(ImagePacket.new(pack_num, total_packets, compressed.slice(start, end)))
		#print("original_size: " + str(image.get_data().size()))
		#print("decomp size: " + str(compressed.decompress_dynamic(image.data.size(), FileAccess.CompressionMode.COMPRESSION_ZSTD)))
	#return res

func packetize_image() -> Array[ImagePacket]:
	var res: Array[ImagePacket] = []
	var GDDSGSG = image.data
	var compressed = Marshalls.raw_to_base64(image.get_data())
	var total_packets = ceil(float(compressed.size()) / PACKET_SIZE)
	for pack_num in range(total_packets):
		var start = pack_num * PACKET_SIZE
		var end = min((pack_num + 1) * PACKET_SIZE, compressed.size())
		res.append(ImagePacket.new(pack_num, total_packets, compressed.slice(start, end)))
		print("original_size: " + str(image.get_data().size()))
		print("decomp size: " + str(Marshalls.base64_to_raw(compressed).size()))
	return res

func empty() -> bool:
	return image == null

func clear():
	image = null
