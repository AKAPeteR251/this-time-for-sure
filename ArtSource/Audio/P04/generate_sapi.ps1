# Local-only narration source. Re-run build_audio.py afterwards; no account/API credential required.
$voiceFolder = $PSScriptRoot
$narrator = New-Object -ComObject SAPI.SpVoice
$narrator.Voice = @($narrator.GetVoices()) | Where-Object { $_.GetDescription() -like '*Huihui*' } | Select-Object -First 1
if ($null -eq $narrator.Voice) { throw 'Microsoft Huihui voice is not installed.' }
$narrator.Rate = 3
$declarations = @('以此真名，缔结星约。', '让群星回应，让命运见证。', '星门，为您而开！')
for ($phraseIndex = 0; $phraseIndex -lt $declarations.Count; $phraseIndex++) {
    $voiceStream = New-Object -ComObject SAPI.SpFileStream
    $voiceStream.Format.Type = 22
    $voiceStream.Open((Join-Path $voiceFolder ('voice_rate3_' + $phraseIndex + '.wav')), 3, $false)
    $narrator.AudioOutputStream = $voiceStream
    [void]$narrator.Speak($declarations[$phraseIndex])
    $voiceStream.Close()
}
