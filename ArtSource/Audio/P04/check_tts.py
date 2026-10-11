from pathlib import Path
import json, urllib.request

result = dict(appSpeechTool=False, actualUsedEngine='Windows SAPI',
    actualUsedVoice='Microsoft Huihui Desktop - Chinese (Simplified)',
    voicesEnumerated=['Microsoft Huihui Desktop - Chinese (Simplified)',
        'Microsoft Zira Desktop - English (United States)', 'Microsoft Haruka Desktop - Japanese',
        'Microsoft David Desktop - English (United States)'])
try:
    info=json.load(urllib.request.urlopen('http://127.0.0.1:8188/object_info',timeout=5))
    result['comfyRegisteredSpeechNodes']=[key for key in info if any(term in key.lower() for term in ['tts','speech','texttospeech'])]
    result['comfyNote']='Registered names do not prove loaded weights or successful generation; no ComfyUI speech generation was run.'
except Exception as error:
    result['comfySpeechCheck']=type(error).__name__
(Path(__file__).resolve().parent/'tts_capabilities.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(result,ensure_ascii=False))
