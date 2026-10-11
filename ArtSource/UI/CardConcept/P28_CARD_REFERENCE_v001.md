# P28-CARD-REFERENCE v001

用户要求用内置图片生成工具制作卡面改版参考。2026-10-11生成，状态：参考，待用户选择；未接入Unity或修改现有角色资产。

输出：`card_sr_thunder_reference_v001.png`。以用户所发雷巫女SR卡截图为角色和布局参考，重新生成了一张2:3卡面视觉方案，并非原立绘像素不变的UI叠层。

方向：紫色晶体细框、统一SR装饰、渐变NEW标签、底部深色渐隐名字区，保留雷电/月光主题。成图为完整合成参考，不能直接当透明边框素材；后续选定方向再拆解美术清单。

方式：内置 image_gen，输入参考截图；未使用CLI/API回退。生成器未提供seed/steps，不推测。

## 最终提示词

Use case: ui-mockup. Create a polished standalone portrait collectible character card UI reference for the anime fantasy gacha game Star Echo, based on the attached current card screenshot. Input image role: character identity and existing card-layout reference. Preserve the character identity: 雷巫女, an adult anime thunder shrine maiden with long dark hair, violet eyes, a white-and-red shrine maiden outfit, holding a ritual wand; same upper-body pose, facial personality, moon and purple lightning theme as the source. Redesign the surrounding card UI to professional game production quality. One single 2:3 portrait card filling the output, straight-on, no perspective, no outside screenshot, no browser or software chrome. Maintain generous visible character artwork, avoid cropping her face, preserve illustration clarity. Thin sophisticated metallic lavender frame with subtle purple luminous inner edge, restrained angular crystal filigree at corners, no thick neon rectangle. Top-left small elegant integrated 'NEW' outlined label with iridescent cyan-pink-violet border, cream lettering, dark translucent fill, aligned neatly inside the card; no tilted red sticker. Top-right 'SR' stylized luminous lavender serif lettering in a small refined inset ornament integrated into the frame, no large black square, no stars or numerical ratings. At the bottom, a graceful dark indigo transparent gradient name area fading upward into the artwork, exact Chinese name '雷巫女' in legible warm white elegant lettering, small tasteful violet thunder emblem at its left. No subtitle, no numbers, no buttons, no other text. The frame, rarity and name area feel like one coherent restrained premium anime fantasy UI. Rich violet lightning and cool moonlight, dark indigo and subtle silver accents, controlled highlights, high readability at small thumbnail size. Keep the character the visual focus, not the decoration. This is a proposed redesign reference, not a UI asset sheet. Exact visible text only: 'NEW', 'SR', '雷巫女'.
