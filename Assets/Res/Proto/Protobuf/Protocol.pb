
�
game_type.protoproto"^
attr_general
name (	Rname
race (	Rrace
class (	Rclass
map (	Rmap"B
position
x (Rx
y (Ry
z (Rz
o (Ro"-
movement!
pos (2.proto.positionRpos"�
	attribute
health (Rhealth
level (Rlevel
exp (Rexp

health_max (R	healthMax
strength (Rstrength
stamina (Rstamina!
attack_power (RattackPower"*
attribute_overview
level (Rlevel"\
attribute_aoi
level (Rlevel
health (Rhealth

health_max (R	healthMax"�
	character
id (Rid-
general (2.proto.attr_generalRgeneral.
	attribute (2.proto.attributeR	attribute+
movement (2.proto.movementRmovement"�
character_agent
id (Rid-
general (2.proto.attr_generalRgeneral.
	attribute (2.proto.attributeR	attribute+
movement (2.proto.movementRmovement"�
character_overview
id (Rid-
general (2.proto.attr_generalRgeneral7
	attribute (2.proto.attribute_overviewR	attribute"�
character_aoi
id (Rid-
general (2.proto.attr_generalRgeneral2
	attribute (2.proto.attribute_aoiR	attribute+
movement (2.proto.movementRmovement"Q
character_aoi_move
id (Rid+
movement (2.proto.movementRmovement"]
character_aoi_attribute
id (Rid2
	attribute (2.proto.attribute_aoiR	attributebproto3
�	
game_msg.protoprotogame_type.proto"

req_ping"

res_ping"F
	req_login#
login_session (RloginSession
token (	Rtoken"%
	res_login
success (Rsuccess"�
res_character_listF
	character (2(.proto.res_character_list.CharacterEntryR	characterW
CharacterEntry
key (Rkey/
value (2.proto.character_overviewRvalue:8"I
req_character_create1
	character (2.proto.attr_generalR	character"O
res_character_create7
	character (2.proto.character_overviewR	character"$
req_character_pick
id (Rid"D
res_character_pick.
	character (2.proto.characterR	character"-
req_move!
pos (2.proto.positionRpos"-
res_move!
pos (2.proto.positionRpos"$

req_combat
target (Rtarget"<

res_combat
target (Rtarget
damage (Rdamage"C
res_aoi_add4

icharacter (2.proto.character_aoiR
icharacter")
req_aoi_add
wantmore (Rwantmore".
res_aoi_remove
	character (R	character"N
res_aoi_update_move7
	character (2.proto.character_aoi_moveR	character"1
req_aoi_update_move
wantmore (Rwantmore"X
res_aoi_update_attribute<
	character (2.proto.character_aoi_attributeR	character"6
req_aoi_update_attribute
wantmore (Rwantmorebproto3
�
login_msg.protoproto"2
res_acknowledgment
	acknumber (	R	acknumber".
req_handshake

client_pub (	R	clientPub"'
res_handshake
secret (	Rsecret"#
req_challenge
hmac (	Rhmac"B
req_auth
username (	Rusername
password (	Rpassword"]
res_auth#
login_session (RloginSession
expire (Rexpire
token (	Rtokenbproto3
�
network.protoproto"`
res_msgresult
session (Rsession
result (Rresult

error_code (R	errorCodebproto3