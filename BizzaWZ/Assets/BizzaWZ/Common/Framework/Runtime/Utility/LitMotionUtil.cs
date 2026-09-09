using System;
using System.Collections.Generic;
using LitMotion;

public static class LitMotionUtil
{
    public static void SetTimeOut(Action action, float delay)
    {
        LMotion.Create(0, 1, delay)
            .WithScheduler(MotionScheduler.InitializationIgnoreTimeScale)
            .WithOnComplete(action)
            .RunWithoutBinding();
    }
}
