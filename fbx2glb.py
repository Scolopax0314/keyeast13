import bpy

# 입력 및 출력 파일 경로 (절대 경로 권장)
fbx_path = "창의체험전/Assets/Models/rigged-hand/Hand_model/hand.fbx"
glb_path = "hand_converted.glb"

# 기존 씬 초기화
bpy.ops.wm.read_factory_settings(use_empty=True)

# FBX 불러오기
bpy.ops.import_scene.fbx(filepath=fbx_path)

# GLB로 저장
bpy.ops.export_scene.gltf(filepath=glb_path, export_format='GLB')
print(f"Exported to {glb_path}")

# Armature 추출 및 본 트리 출력
armature = next((obj for obj in bpy.data.objects if obj.type == 'ARMATURE'), None)

def print_bone_tree(bones, indent=0):
    for bone in bones:
        print("  " * indent + f"- {bone.name}")
        print_bone_tree(bone.children, indent + 1)

if armature:
    print("Bone Hierarchy:")
    print_bone_tree(armature.data.bones)
else:
    print("No armature found.")
