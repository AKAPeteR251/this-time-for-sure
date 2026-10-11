# P15-NEBULA-SLOW

用户已认可P14效果，只要求速度再慢一点。本次仅调整星云变化时间倍率 `nebulaEvolution: 0.09 → 0.06`，扭曲/起伏/内部变化节奏降低约33%，周期延长约50%。横向漂移0.0022、扭曲强度0.035、起伏幅度0.025及粉尘/构图/亮度均保持P14。

修改 `这次一定出/Assets/_Game/Editor/P14NebulaDistortSetup.cs` 的准备值，通过既有Unity Editor准备入口保存 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`。不改Shader和交互，不覆盖已认可的P14实录。本次是参数微调，不新增测试或重复整批验证；核对Unity保存后的配置值。

涉及Unity概念：演化时间由累计非缩放时间乘以速度参数，减小参数会减慢所有采用该时间的扭曲/起伏；漂移使用独立参数，因此不受影响。调参入口为P05FeelConfig.asset中的nebulaEvolution，若恢复P14原速度设0.09。

状态：已验收。用户在P16开始前明确“星云的效果验收通过了”。参数nebulaEvolution=0.06、nebulaDrift=0.0022、nebulaBillow=0.025、nebulaDistortion=0.035保持；P16实际Main Play也使用同一版星云，无Shader绘制错误。P15当批仅保存配置，没有新实录；P14视频代表减速前版本，P16实录可看当前速度。
