#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.Serialize;

namespace EntryEngine
{
    public enum EParticleStreamType
    {
        出生 = 0,
        一次性 = 1,
        条件 = 2,
        变化 = 3,
    }
    public class ASummaryP : ASummary
    {
        public EParticleStreamType Type { get; private set; }
        public ASummaryP(string name, string note, EParticleStreamType type) : base(name, note)
        {
            this.Type = type;
        }
    }

	/*
	 * 1. 时间轴动画 Timeline
	 * 2. 粒子系统 Particle System
	 * 3. 帧动画 Sequence
	 * 4. 骨骼动画 Avatar
	 */
    [AReflexible]public abstract class ParticleStream
    {
        /// <summary>当前流更新一次后下次粒子将跳到当前流的后一个流处开始执行</summary>
        public bool Skip;
        /// <summary>当前流的子流数量。当Update返回Flase时，Child为0则停止当前层次的流，Child不为0则仅跳过所有子流</summary>
        public byte Child;

        /// <summary>更新粒子表现</summary>
        /// <param name="p">当前粒子</param>
        /// <param name="ps">粒子所属粒子系统，可以用于创建删除粒子</param>
        /// <param name="elapsed">经过时间，单位ms</param>
        /// <returns>是否继续更新后面的流</returns>
        public abstract bool Update(Particle p, ParticleEmitter ps, float elapsed);
        public virtual void Reset() { }
        public virtual ParticleStream Clone() { return this; }
    }
    /// <summary>若Random都采用同一个种子，会出现各种规律现象</summary>
    [AReflexible]public abstract class PSRandom : ParticleStream
    {
        private _RANDOM.Random random;
        [ASummary("随机种子", "可以让随机效果微微改变")]
        public int Seed;

        protected _RANDOM.Random Random
        {
            get
            {
                if (random == null)
                    random = new RandomDotNet(Seed);
                return random;
            }
        }

        protected PSRandom()
        {
            Seed = _RANDOM.Next();
        }

        public override void Reset()
        {
            random = null;
        }
    }

    // BORN
    [ASummaryP("粒子定时出生", "", EParticleStreamType.出生)]
    [AReflexible]public class PBByTime : ParticleStream
    {
        private float _remain;
        [ASummary("秒粒子个数", "1秒产生粒子的个数")]
        public int BornPerSecond = 60;
        [ASummary("粒子上限", "产生出的粒子数量的上限，为0时不限制粒子产生")]
        public ushort LimitCount;
        private float _time;
        [ASummary("开始时间(秒)", "开始产生粒子的时间，这个时间之前不会产生粒子")]
        public float StartTime;
        [ASummary("结束时间(秒)", "停止产生粒子的时间，这个时间之后不会产生粒子\n-1: 不停产生粒子\n0: \"粒子上限\"为0时，直接产生出\"秒粒子个数\"个粒子，否则直接产生\"粒子上限\"个粒子")]
        public float EndTime = -1;

        public PBByTime()
        {
        }
        public PBByTime(int bornPerSecond)
        {
            this._remain = 0;
            this.BornPerSecond = bornPerSecond;
            this.LimitCount = 0;
        }
        public PBByTime(int bornPerSecond, ushort limitCount)
        {
            this._remain = 0;
            this.BornPerSecond = bornPerSecond;
            this.LimitCount = limitCount;
        }

        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            int born = 0;
            float previous = _time;
            _time += elapsed;
            if (_time >= StartTime)
            {
                if (EndTime == 0)
                {
                    if (previous <= StartTime)
                    {
                        if (LimitCount != 0)
                            born = LimitCount;
                        else if (BornPerSecond != 0)
                            born = BornPerSecond;
                    }
                }
                else if (EndTime < 0 || _time < EndTime)
                {
                    if (LimitCount <= 0 || ps.Count < LimitCount)
                    {
                        // 根据每秒出生率计算出生数
                        float temp = elapsed * BornPerSecond + _remain;
                        born = (int)(temp);
                        _remain = temp - born;

                        if (LimitCount > 0)
                            born = _MATH.Min(LimitCount - ps.Count, born);
                    }
                }
            }
            if (born > 0)
                for (int i = 0; i < born; i++)
                    ps.CreateParticle();
            return false;
        }
        public override void Reset()
        {
            _remain = 0;
            _time = 0;
        }
        public override ParticleStream Clone()
        {
            PBByTime clone = new PBByTime();
            clone.BornPerSecond = this.BornPerSecond;
            clone.StartTime = this.StartTime;
            clone.EndTime = this.EndTime;
            clone.LimitCount = this.LimitCount;
            clone.Skip = this.Skip;
            clone.Child = this.Child;
            return clone;
        }
    }

    // APPEAR | SKIP
    [ASummaryP("跳过", "本节点执行过一次后，除了根节点，本节点前面的所有节点都将不再执行", EParticleStreamType.一次性)]
    [AReflexible]public class PSSkip : ParticleStream
    {
        public PSSkip() { Skip = true; }
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            return true;
        }
    }
    [AReflexible]public abstract class PSRandomSkip : PSRandom
    {
        protected PSRandomSkip() { Skip = true; }
    }
    [ASummaryP("存在时间", "粒子出生后持续存在的时间，时间后将消失", EParticleStreamType.一次性)]
    [AReflexible]public class PSLifecycle : PSRandomSkip
    {
        [ASummary("存在时间(秒)", "粒子从出生后持续存在的时间，时间后将消失")]
        public float Lifecycle = 2f;
        [ASummary("浮动值", "\"粒子存在时间(秒)\"上下浮动的百分比")]
        public float VaryP = 0.25f;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Life = Lifecycle;
            if (VaryP != 0)
                p.Life *= 1 + Random.NextSign() * Random.Next(VaryP);
            return true;
        }
    }
    [ASummaryP("文字", "让粒子显示文字", EParticleStreamType.一次性)]
    [AReflexible]public class PSFont : PSSkip
    {
        [ASummary("字体", "显示文字用的字体")]
        public FONT Font = FONT.Default;
        [ASummary("字体大小", "")]
        public float FontSize;
        [ASummary("文字", "显示的文字内容")]
        public string Text;
        [ASummary("对齐", "显示的文字内容的对齐方式")]
        public UI.EPivot Pivot;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (FontSize > 0)
                Font.FontSize = FontSize;
            p.Font = Font;
            p.Text = Text;
            if (!string.IsNullOrEmpty(Text))
            {
                VECTOR2 size = Font.MeasureString(Text);
                p.TextOffset = UI.UIElement.CalcPivotPoint(size, Pivot);
            }
            return true;
        }
    }
    [ASummaryP("图片", "让粒子显示图片，若是序列帧等动态图片，动作将是同步的", EParticleStreamType.一次性)]
    [AReflexible]public class PSTex : PSSkip
    {
        [ASummary("图片", "让粒子显示图片，右键左侧标题，可以切换默认图片")]
        public TEXTURE Texture = TEXTURE.Pixel;
        [ASummary("颜色", "以叠加颜色的方式显示图片")]
        public COLOR Color = COLOR.White;
        [ASummary("锚点", "图片旋转时的锚点，填0~1")]
        public VECTOR2 Origin = new VECTOR2(0.5f, 0.5f);
        [ASummary("反转", "图片显示反转，可以不反转，水平反转，垂直反转，水平|垂直反转")]
        public EFlip Flip;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Texture = Texture;
            p.Color = Color;
            p.Origin = Origin;
            p.Flip = Flip;
            return true;
        }
    }
    [ASummaryP("拷贝图片", "让粒子显示图片，若是序列帧等动态图片，每个粒子的当前帧是不一样的, EParticleStreamType.出现", EParticleStreamType.一次性)]
    [AReflexible]public class PSTexClone : PSTex
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            base.Update(p, ps, elapsed);
            p.Texture = (TEXTURE)Texture.Cache();
            return true;
        }
    }
    [ASummaryP("位置：坐标", "粒子出生的固定坐标", EParticleStreamType.一次性)]
    [AReflexible]public class PSPosPoint : PSSkip
    {
        [ASummary("坐标", "右键可以在屏幕上点击选择一个点")]
        public VECTOR2 Position;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Position.X = Position.X;
            p.Position.Y = Position.Y;
            return true;
        }
    }
    [ASummaryP("位置：矩形", "粒子出生的矩形区域", EParticleStreamType.一次性)]
    [AReflexible]public class PSPosRectangle : PSRandomSkip
    {
        [ASummary("矩形区域", "右键可以在屏幕拖拽选择一个矩形区域")]
        public RECT Area = new RECT(-50, -50, 100, 100);
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float x = Random.Next(Area.Width);
            float y = Random.Next(Area.Height);
            p.Position.X = x + Area.X;
            p.Position.Y = y + Area.Y;
            return true;
        }
    }
    [ASummaryP("位置：圆形", "粒子出生的圆形区域", EParticleStreamType.一次性)]
    [AReflexible]public class PSPosCircle : PSRandomSkip
    {
        [ASummary("矩形区域", "暂不支持视图操作，只能自己填数值")]
        public CIRCLE Area = new CIRCLE(50);
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float radian = Random.NextRadian();
            float r = Random.Next(Area.R);
            CIRCLE.ParametricEquation(ref Area.C, r, radian, out p.Position);
            return true;
        }
    }
    [ASummaryP("绝对位置", "粒子系统位置发生变化时，之前出生的粒子保留在原来的位置\n视图内右键拖动可以看到效果", EParticleStreamType.一次性)]
    [AReflexible]public class PSPosAbsolute : PSSkip
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.PosMode = 1;
            return true;
        }
    }
    [ASummaryP("运动", "粒子运动的速度和方向", EParticleStreamType.一次性)]
    [AReflexible]public class PSSpeed : PSRandomSkip
    {
        /// <summary>每秒移动的像素</summary>
        [ASummary("速度", "每秒移动的像素值")]
        public float Speed = 300;
        [ASummary("速度浮动", "\"速度\"上下浮动的百分比")]
        public float VaryP;
        [ASummary("方向", "0~360°，右边为0，顺时针增大")]
        public float Direction;
        [ASummary("方向浮动", "\"方向\"确定后，上下浮动的度数")]
        public float VaryV = 22.5f;

        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Speed != 0)
            {
                p.speed = Speed;
                if (VaryP != 0)
                    p.speed *= 1 + Random.NextSign() * Random.Next(VaryP);
            }

            p.direction = Direction;
            if (VaryV != 0)
                p.direction += Random.NextSign() * Random.Next(VaryV);
            //p.direction = _MATH.ToRadian(p.direction);

            p.ResetVector();
            return true;
        }
    }
    [ASummaryP("大小", "粒子的尺寸", EParticleStreamType.一次性)]
    [AReflexible]public class PSScale : PSRandomSkip
    {
        [ASummary("缩放", "粒子缩放的百分比")]
        public float Scale = 1;
        [ASummary("缩放浮动", "\"缩放\"上下浮动的百分比")]
        public float VaryP;

        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float scale = Scale;
            if (VaryP != 0)
                scale *= 1 + Random.NextSign() * Random.Next(VaryP);
            p.Scale.X = scale;
            p.Scale.Y = scale;
            return true;
        }
    }
    [ASummaryP("旋转", "粒子的旋转", EParticleStreamType.一次性)]
    [AReflexible]public class PSRotation : PSRandomSkip
    {
        [ASummary("角度", "粒子显示的角度")]
        public float Rotation;
        [ASummary("角度浮动", "\"角度\"确定后，上下浮动的角度")]
        public float VaryV = 180;
        [ASummary("旋转", "每秒旋转的角度，让粒子显示的角度不断变化")]
        public float Rotate;
        [ASummary("旋转浮动", "\"旋转\"确定后，上下浮动的度数")]
        public float VaryRotateV;
        [ASummary("朝向运动", "可以让粒子的显示角度朝向运动角度")]
        public bool Forward;

        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float angle = 0;
            if (Forward)
                angle = p.direction;
            angle += Rotation;
            if (VaryV != 0)
                angle += Random.NextSign() * Random.Next(VaryV);
            p.Rotation = _MATH.ToRadian(angle);

            angle = Rotate;
            if (VaryRotateV != 0)
                angle += Random.NextSign() * Random.Next(VaryRotateV);
            p.Rotate = _MATH.ToRadian(angle);
            return true;
        }
    }
    [Flags]public enum EPSColor : byte
    {
        无 = 0,
        红 = 1,
        绿 = 2,
        蓝 = 4,
        不透明 = 8,
        //All = 15,
    }
    [ASummaryP("颜色", "", EParticleStreamType.一次性)]
    [AReflexible]public class PSColor : PSRandomSkip
    {
        [ASummary("起始色", "RGBA分别会从起始色随机到结束色，起始色的值大于结束色时，取起始色的值")]
        public COLOR From = COLOR.Black;
        [ASummary("结束色", "")]
        public COLOR To = COLOR.White;
        [ASummary("颜色同步", "同步的两个或多个颜色，先随机一个(优先顺序红>绿>蓝>不透明)，其它的跟前面随机的一样")]
        public EPSColor Same = EPSColor.无;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            bool r = (Same & EPSColor.红) != EPSColor.无;
            bool g = (Same & EPSColor.绿) != EPSColor.无;
            bool b = (Same & EPSColor.蓝) != EPSColor.无;
            bool a = (Same & EPSColor.不透明) != EPSColor.无;

            if (From.R <= To.R)
                p.Color.R = (byte)Random.Next(From.R, To.R);
            else
                p.Color.R = From.R;

            if (g && r)
                p.Color.G = p.Color.R;
            else
            {
                if (From.G <= To.G)
                    p.Color.G = (byte)Random.Next(From.G, To.G);
                else
                    p.Color.G = From.G;
            }

            if (b && (r || g))
            {
                if (r)
                    p.Color.B = p.Color.R;
                else
                    p.Color.B = p.Color.G;
            }
            else
            {
                if (From.B <= To.B)
                    p.Color.B = (byte)Random.Next(From.B, To.B);
                else
                    p.Color.B = From.B;
            }

            if (a && (r || g || b))
            {
                if (r)
                    p.Color.A = p.Color.R;
                else if (g)
                    p.Color.A = p.Color.G;
                else
                    p.Color.A = p.Color.B;
            }
            else
            {
                if (From.A <= To.A)
                    p.Color.A = (byte)Random.Next(From.A, To.A);
                else
                    p.Color.A = From.A;
            }
            return true;
        }
    }

    // CONDITION
    [ASummaryP("随机子节点", "每次随机其子节点的其中一个对粒子进行作用", EParticleStreamType.条件)]
    [AReflexible]public class PSRandomChild : PSRandom
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Child > 0)
            {
                int offset;
                if (Child == 1)
                    offset = 1;
                else
                    offset = Random.Next(Child) + 1;
                ps.Flow[ps._stream + offset].Update(p, ps, elapsed);
                return false;
            }
            return true;
        }
    }
    [ASummaryP("概率子节点", "每次按照一定概率执行其中的所有子节点", EParticleStreamType.条件)]
    [AReflexible]public class PSProbability : PSRandom
    {
        [ASummary("概率", "0~100，代表需要执行子节点的概率")]
        public byte Percent = 50;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Child > 0 && Percent > 0)
            {
                if (Percent >= 100)
                    return true;
                return Random.Next(100) < Percent;
            }
            return true;
        }
    }
    [ASummaryP("与", "任一子节点不符合条件自身就不符合条件", EParticleStreamType.条件)]
    [AReflexible]public class PSAnd : ParticleStream
    {
        [ASummary("取反", "取反后，变成任一子节点不符合条件自身就\"符合\"条件")]
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Child > 0)
            {
                for (int i = 0; i < Child; i++)
                    if (!ps.Flow[ps._stream + i].Update(p, ps, elapsed))
                        return Not;
            }
            return true;
        }
    }
    [ASummaryP("或", "任一子节点符合条件自身就符合条件", EParticleStreamType.条件)]
    [AReflexible]public class PSOr : ParticleStream
    {
        [ASummary("取反", "取反后，变成任一子节点符合条件自身就\"不符合\"条件")]
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Child > 0)
            {
                for (int i = 0; i < Child; i++)
                    if (ps.Flow[ps._stream + i].Update(p, ps, elapsed))
                        return !Not;
                return Not;
            }
            return true;
        }
    }
    [ASummaryP("顺序子节点", "顺序执行子节点，不符合条件后停止", EParticleStreamType.条件)]
    [AReflexible]public class PSOrder : ParticleStream
    {
        [ASummary("取反", "取反后，变成\"符合\"条件后停止")]
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (Child > 0)
            {
                for (int i = 0; i < Child; i++)
                    if (ps.Flow[ps._stream + i].Update(p, ps, elapsed) == Not)
                        return false;
            }
            return true;
        }
    }
    [ASummaryP("范围：矩形", "粒子是否在矩形范围内", EParticleStreamType.条件)]
    [AReflexible]public class PSInArea : ParticleStream
    {
        [ASummary("矩形范围", "右键可以在屏幕拖拽选择一个矩形区域")]
        public RECT Area = new RECT(-50, -50, 100, 100);
        [ASummary("取反", "取反后，变成粒子是否\"不\"在矩形范围内")]
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            return Area.Contains(p.Position.X, p.Position.Y) != Not;
        }
    }
    [ASummaryP("范围：圆形", "粒子是否在圆形范围内", EParticleStreamType.条件)]
    [AReflexible]public class PSInCircle : ParticleStream
    {
        [ASummary("圆形范围", "暂不支持视图操作，只能自己填数值")]
        public CIRCLE Area = new CIRCLE(50);
        [ASummary("取反", "取反后，变成粒子是否\"不\"在圆形范围内")]
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            return Area.Contains(p.Position.X, p.Position.Y) != Not;
        }
    }
    [ASummaryP("范围：存在时间", "粒子存在时间是否在范围内", EParticleStreamType.条件)]
    [AReflexible]public class PSTimer : ParticleStream
    {
        //private float _time;
        [ASummary("开始时间(秒)", "粒子存在时间需要大于等于开始时间")]
        public float StartTime;
        [ASummary("时间间隔(秒)", "在时间范围内每间隔时间符合一次条件，-1时每次都符合条件")]
        public float Interval = -1;
        [ASummary("结束时间(秒)", "粒子存在时间需要小于等于开始时间，-1时不检测结束时间")]
        public float EndTime = -1;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float _time = p.Age;
            float temp = _time - StartTime;
            _time += elapsed;
            if (_time >= StartTime && (EndTime < 0 || elapsed <= EndTime))
            {
                if (Interval <= 0)
                    return true;
                else
                    return (int)((_time - StartTime) / Interval) != (int)(temp / Interval);
            }
            return false;
        }
    }

    [AReflexible]public enum EPSCheck : byte
    {
        等于,
        大于,
        大于等于,
        小于,
        小于等于,
    }
    [AReflexible]public abstract class PSCheck : ParticleStream
    {
        [ASummary("比较值", "")]
        public float Value;
        [ASummary("比较符", "")]
        public EPSCheck OP;

        public abstract float GetParticleValue(Particle p);
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float value = GetParticleValue(p);
            switch (OP)
            {
                case EPSCheck.等于: return p.speed == Value;
                case EPSCheck.大于: return p.speed > Value;
                case EPSCheck.大于等于: return p.speed >= Value;
                case EPSCheck.小于: return p.speed < Value;
                case EPSCheck.小于等于: return p.speed <= Value;
                default: return true;
            }
        }
    }
    [ASummaryP("范围：速度", "粒子移动速度是否在范围内，比较值为每秒移动的像素值", EParticleStreamType.条件)]
    [AReflexible]public class PSSpeedCheck : PSCheck
    {
        public override float GetParticleValue(Particle p)
        {
            return p.speed;
        }
    }
    [ASummaryP("范围：大小", "粒子大小是否在范围内，比较值为粒子的大小", EParticleStreamType.条件)]
    [AReflexible]public class PSScaleCheck : PSCheck
    {
        [ASummary("横向", "")]
        public bool ScaleX = true;

        public override float GetParticleValue(Particle p)
        {
            if (ScaleX)
                return p.Scale.X;
            else
                return p.Scale.Y;
        }
    }


    // MOTION
    [ASummaryP("改变：存在时间", "粒子存在时间改变", EParticleStreamType.变化)]
    [AReflexible]public class PSLifeAdd : ParticleStream
    {
        [ASummary("存在时间(秒)", "")]
        public float Lifecycle;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Life += Lifecycle;
            return true;
        }
    }
    [AReflexible]public abstract class PSPropertyAdd : PSRandom
    {
        [ASummary("随机起始", "随机一个固定的改变值")]
        public float VaryFrom;
        [ASummary("随机结束", "随机一个固定的改变值")]
        public float Vary;
        [ASummary("随机起始2", "随机一个原本数值为基数的百分比的改变值")]
        public float VaryPFrom;
        [ASummary("随机结束2", "随机一个原本数值为基数的百分比的改变值")]
        public float VaryP;
        [ASummary("正负", "改变的数值是否需要随机正负")]
        public bool Sign;
        public float VaryValue(float value)
        {
            float vary = 0;
            //if (VaryFrom < Vary && (VaryFrom != Vary || Vary != 0))
            if (VaryFrom != Vary || Vary != 0)
            {
                vary = Random.Next(VaryFrom, Vary);
            }
            //if (VaryPFrom < VaryP && (VaryPFrom != VaryP || VaryP != 0))
            if (VaryPFrom != VaryP || VaryP != 0)
            {
                vary += value * Random.Next(VaryPFrom, VaryP);
            }
            if (vary == 0)
                return 0;
            if (Sign)
                vary *= Random.NextSign();
            return vary;
        }
    }
    [ASummaryP("改变：速度", "粒子运动速度改变", EParticleStreamType.变化)]
    [AReflexible]public class PSSpeedAdd : PSPropertyAdd
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float vary = VaryValue(p.speed);
            if (vary != 0)
            {
                p.Speed += vary;
            }
            return true;
        }
    }
    [ASummaryP("改变：方向", "粒子运动方向改变", EParticleStreamType.变化)]
    [AReflexible]public class PSDirectionAdd : PSPropertyAdd
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float vary = VaryValue(p.direction);
            if (vary != 0)
            {
                p.Direction += vary;
            }
            return true;
        }
    }
    [ASummaryP("改变：旋转", "粒子旋转方向改变", EParticleStreamType.变化)]
    [AReflexible]public class PSRotateAdd : PSPropertyAdd
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Rotate += VaryValue(p.Rotate);
            return true;
        }
    }
    [ASummaryP("改变：朝向", "粒子朝向改变", EParticleStreamType.变化)]
    [AReflexible]public class PSRotationForward : ParticleStream
    {
        [ASummary("朝向偏移", "粒子朝向运动方向后增加的一个偏移角度")]
        public float Offset;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Rotation = _MATH.ToRadian(p.direction + Offset);
            return true;
        }
    }
    [ASummaryP("改变：颜色", "粒子颜色改变", EParticleStreamType.变化)]
    [AReflexible]public class PSColorAdd : PSPropertyAdd
    {
        [ASummary("变色", "指定需要改变的颜色")]
        public EPSColor Change = EPSColor.不透明;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (VaryP == 0 && VaryPFrom == 0)
            {
                float vary = VaryValue(0);
                if (vary != 0)
                {
                    if ((Change & EPSColor.红) != EPSColor.无)
                        p.Color.R = _MATH.InByte(p.Color.R + vary);
                    if ((Change & EPSColor.绿) != EPSColor.无)
                        p.Color.G = _MATH.InByte(p.Color.G + vary);
                    if ((Change & EPSColor.蓝) != EPSColor.无)
                        p.Color.B = _MATH.InByte(p.Color.B + vary);
                    if ((Change & EPSColor.不透明) != EPSColor.无)
                        p.Color.A = _MATH.InByte(p.Color.A + vary);
                }
            }
            else
            {
                if ((Change & EPSColor.红) != EPSColor.无)
                    p.Color.R = _MATH.InByte(p.Color.R + VaryValue(p.Color.R));
                if ((Change & EPSColor.绿) != EPSColor.无)
                    p.Color.G = _MATH.InByte(p.Color.G + VaryValue(p.Color.G));
                if ((Change & EPSColor.蓝) != EPSColor.无)
                    p.Color.B = _MATH.InByte(p.Color.B + VaryValue(p.Color.B));
                if ((Change & EPSColor.不透明) != EPSColor.无)
                    p.Color.A = _MATH.InByte(p.Color.A + VaryValue(p.Color.A));
            }
            return true;
        }
    }
    [ASummaryP("改变：大小", "粒子大小改变", EParticleStreamType.变化)]
    [AReflexible]public class PSScaleAdd : PSPropertyAdd
    {
        [ASummary("横向缩放", "")]
        public bool ScaleX = true;
        [ASummary("纵向缩放", "")]
        public bool ScaleY = true;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (ScaleX && ScaleY)
            {
                float value = VaryValue(p.Scale.X);
                p.Scale.X += value;
                p.Scale.Y += value;
            }
            else if (ScaleX)
                p.Scale.X += VaryValue(p.Scale.X);
            else if (ScaleY)
                p.Scale.Y += VaryValue(p.Scale.Y);
            return true;
        }
    }

    [ASummaryP("运动：风", "平行风使粒子按照风的方向运动", EParticleStreamType.变化)]
    [AReflexible]public class PSWind : ParticleStream
    {
        [ASummary("风", "向量代表了风的大小和方向，右键可以在屏幕上点击选择一个点")]
        public VECTOR2 Force;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Position.X += Force.X * elapsed;
            p.Position.Y += Force.Y * elapsed;
            return true;
        }
    }

    // DISAPPEAR
    [ASummaryP("粒子消失", "让粒子直接消失", EParticleStreamType.一次性)]
    [AReflexible]public class PSDie : ParticleStream
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            ps.RemoveParticle(p);
            return false;
        }
    }

    // PARTICLE SYSTEM
    public class Particle : PoolItem
    {
        public FONT Font;
        public string Text;
        public VECTOR2 TextOffset;
        public TEXTURE Texture;
        public VECTOR2 Position;
        public COLOR Color = COLOR.White;
        public VECTOR2 Scale = new VECTOR2(1);
        public float Rotation;
        public VECTOR2 Origin;
        public EFlip Flip;

        // 每秒移动的像素
        internal float speed;
        internal float direction;
        public float Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                ResetVector();
            }
        }
        /// <summary>力的方向（角度）</summary>
        public float Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                ResetVector();
            }
        }
        internal VECTOR2 Vector;
        /// <summary>绝对坐标，不会跟随粒子系统位移，但会跟随粒子系统的其它矩阵变化</summary>
        public VECTOR2 AbsolutePosition;
        /// <summary>0: 相对坐标 1: 绝对坐标尚未确定坐标 2: 绝对坐标已确定坐标</summary>
        public byte PosMode;
        /// <summary>每秒旋转的角度</summary>
        public float Rotate;

        /// <summary>粒子存在时间（秒）</summary>
        public float Life;
        protected internal float Age;
        internal int StreamIndex;

        internal void ResetVector()
        {
            float radian = _MATH.ToRadian(direction);
            Vector.X = (float)Math.Cos(radian) * speed;
            Vector.Y = (float)Math.Sin(radian) * speed;
        }
    }
    [AReflexible]public class ParticleSystem : TEXTURE, IUpdatable
    {
        private List<ParticleEmitter> emitters = new List<ParticleEmitter>();
        private bool _updated;
        private float _elapsed;
        /// <summary>粒子持续时间(s)</summary>
        public float Duration;

        public ParticleEmitter[] Emitters
        {
            get { return emitters.ToArray(); }
        }
        public int EmittersCount
        {
            get { return emitters.Count; }
        }
        public override int Width
        {
            get { return 1; }
        }
        public override int Height
        {
            get { return 1; }
        }
        public float Elapsed
        {
            get { return _elapsed; }
        }
        public override bool IsEnd
        {
            get { return Duration > 0 && _elapsed >= Duration; }
        }
        public override bool IsDisposed
        {
            get { return emitters == null; }
        }

        public ParticleSystem()
        {
        }
        public ParticleSystem(IEnumerable<ParticleEmitter> emitters)
        {
            foreach (var item in emitters)
                AddEmitter(item);
        }
        public ParticleSystem(StructureParticleSystem structure)
        {
            this.Duration = structure.Duration;
            if (structure.Emitters != null)
                for (int i = 0; i < structure.Emitters.Length; i++)
                    AddEmitter(structure.Emitters[i]);
        }

        public bool AddEmitter(ParticleEmitter emitter)
        {
            if (emitter == null || emitters.Contains(emitter))
                return false;
            emitters.Add(emitter);
            return true;
        }
        public bool RemoveEmitter(ParticleEmitter emitter)
        {
            return emitters.Remove(emitter);
        }
        public void SetElapsed(float value)
        {
            SetElapsed(value, value * 0.005f);
        }
        public void SetElapsed(float value, float fps)
        {
            if (value <= 0)
            {
                Reset();
                return;
            }
            if (fps <= 0)
                throw new ArgumentException("fps must bigger than 0.");
            if (value <= _elapsed)
                Reset();

            while (_elapsed < value)
                Update(fps);
            this._elapsed = value;
        }
        public void Update(float elapsed)
        {
            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Update(elapsed);
            _updated = true;
            _elapsed += elapsed;
        }
        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (!_updated && EntryService.Instance != null)
                Update(EntryService.Instance.GameTime.ElapsedSecond);

            if (emitters.Count > 0)
            {
                MATRIX2x3 matrix;
                __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
                graphics.BeginFromPrevious(matrix);
                var apos = matrix.Translation;
                for (int i = 0; i < emitters.Count; i++)
                    emitters[i].Draw(graphics, apos);
                graphics.End();
            }

            _updated = false;
            return true;
        }
        public void Reset()
        {
            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Reset();
            _elapsed = 0;
        }
        public override Content Cache()
        {
            var cache = new ParticleSystem();
            cache.Duration = this.Duration;
            for (int i = 0; i < emitters.Count; i++)
                cache.emitters.Add(emitters[i].Clone());
            cache._Key = this._Key;
            return cache;
        }
        protected internal override void InternalDispose()
        {
            emitters = null;
        }
    }
    [AReflexible]public struct StructureParticleSystem
    {
        public float Duration;
        public ParticleEmitter[] Emitters;
        public StructureParticleSystem(ParticleSystem ps)
        {
            this.Duration = ps.Duration;
            this.Emitters = ps.Emitters;
        }
    }
    public class ParticleEmitter : IUpdatable
    {
        private static Particle nullParticle = new Particle();

        private Pool<Particle> particles = new Pool<Particle>();
        private int _updated;
        /// <summary>经过的秒数</summary>
        private float _elapsed;
        internal int _stream;
        public ParticleStream[] Flow;

        public int Count
        {
            get { return particles.Count; }
        }

        public Particle CreateParticle()
        {
            Particle particle;
            if (particles.Allot(out particle) == -1)
            {
                particle = new Particle();
                particles.Add(particle);
            }
            particle.Texture = null;
            particle.Position.X = 0;
            particle.Position.Y = 0;
            particle.Color.R = 0;
            particle.Color.G = 0;
            particle.Color.B = 0;
            particle.Color.A = 0;
            particle.Scale.X = 1;
            particle.Scale.Y = 1;
            particle.Rotation = 0;
            particle.Origin.X = 0;
            particle.Origin.Y = 0;
            particle.Flip = EFlip.None;
            particle.speed = 0;
            particle.direction = 0;
            particle.Vector.X = 0;
            particle.Vector.Y = 0;
            particle.PosMode = 0;
            particle.Rotate = 0;
            particle.Life = 0;
            particle.StreamIndex = _stream + 1;
            particle.Age = 0;
            return particle;
        }
        public void RemoveParticle(Particle particle)
        {
            if (particle == null)
                throw new ArgumentNullException("particle");
            particles.RemoveAt(particle);
        }
        public void Update(float elapsed)
        {
            _updated = GameTime.Time.FrameID;
            if (Flow == null || Flow.Length == 0 || elapsed == 0)
                return;

            _stream = 0;
            Flow[0].Update(nullParticle, this, elapsed);

            this._elapsed = elapsed;
            particles.For(ParticleUpdate);
        }
        private void ParticleUpdate(Particle p)
        {
            if (Flow != null)
            {
                int end = Flow.Length;
                for (int i = p.StreamIndex; i < end; i++)
                {
                    var flow = Flow[i];
                    if (flow != null)
                    {
                        _stream = i;

                        if (flow.Skip)
                            // 子流将被调用一次后跳过，可子流也可能是跳过流，此时子流的跳过索引可能比较小
                            p.StreamIndex = _MATH.Max(_stream + 1 + flow.Child, p.StreamIndex);

                        if (!flow.Update(p, this, _elapsed))
                        {
                            if (flow.Child == 0)
                            {
                                //for (int j = i; j > 0; j--)
                                //{
                                //    if (Flow[j].Child != 0)
                                //    {
                                //        i = j + 1 + Flow[j].Child;
                                //        break;
                                //    }
                                //}
                                break;
                            }
                            else
                                i += flow.Child;
                        }
                    }
                }
            }

            //if (p.Speed != 0)
            //    CIRCLE.ParametricEquation(ref p.Position, p.Speed * _elapsed, p.Direction, out p.Position);
            if (p.Vector.X != 0 || p.Vector.Y != 0)
            {
                p.Position.X += p.Vector.X * _elapsed;
                p.Position.Y += p.Vector.Y * _elapsed;
            }
            p.Rotation += p.Rotate * _elapsed;

            p.Age += _elapsed;
            if (p.Age >= p.Life)
            {
                particles.RemoveAt(p);
            }
        }
        internal void Draw(GRAPHICS graphics, VECTOR2 apos)
        {
            particles.For(sprite =>
            {
                VECTOR2 pos;
                if (sprite.PosMode == 1)
                {
                    sprite.AbsolutePosition = apos;
                    sprite.PosMode = 2;
                }
                if (sprite.PosMode == 2)
                {
                    pos.X = sprite.Position.X + sprite.AbsolutePosition.X - apos.X;
                    pos.Y = sprite.Position.Y + sprite.AbsolutePosition.Y - apos.Y;
                }
                else
                {
                    pos.X = sprite.Position.X;
                    pos.Y = sprite.Position.Y;
                }
                if (sprite.Texture != null)
                {
                    graphics.Draw(sprite.Texture, pos, GRAPHICS.NullSource, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Flip);
                }
                if (sprite.Font != null && !string.IsNullOrEmpty(sprite.Text))
                {
                    pos.X -= sprite.TextOffset.X;
                    pos.Y -= sprite.TextOffset.Y;
                    graphics.Draw(sprite.Font, sprite.Text, pos, sprite.Color, 1);
                }
            });
        }
        //private bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        //{
        //    if (_updated != GameTime.Time.FrameID)
        //    {
        //        _updated = GameTime.Time.FrameID;
        //        Update(GameTime.Time.ElapsedSecond);
        //    }

        //    if (particles.Count > 0)
        //    {
        //        MATRIX2x3 matrix;
        //        __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
        //        graphics.BeginFromPrevious(matrix);
        //        VECTOR2 apos = matrix.Translation;
        //        Draw(graphics, apos);
        //        graphics.End();
        //    }

        //    return true;
        //}
        public void Reset()
        {
            _updated = 0;
            particles.ClearToFree();
            if (Flow != null)
                for (int i = 0; i < Flow.Length; i++)
                    if (Flow[i] != null)
                        Flow[i].Reset();
        }

        public ParticleEmitter Clone()
        {
            var cache = new ParticleEmitter();
            if (this.Flow != null)
            {
                int count = this.Flow.Length;
                cache.Flow = new ParticleStream[count];
                for (int i = 0; i < count; i++)
                {
                    cache.Flow[i] = this.Flow[i].Clone();
                }
            }
            return cache;
        }
    }
    public class PipelineParticle : ContentPipeline
    {
        public const string FILE_TYPE = "ps";

        public override IEnumerable<string> SuffixProcessable
        {
            get { yield break; }
        }
        public override string FileType
        {
            get { return FILE_TYPE; }
        }

        protected internal override Content Load(string file)
        {
            byte[] buffer = IO.ReadByte(file);
            ByteRefReader reader = new ByteRefReader(buffer);
            reader.AddOnDeserialize(TEXTURE.Deserializer(Manager, null));
            reader.AddOnDeserialize(FONT.Deserializer(Manager, null));
            return new ParticleSystem(reader.ReadObject<StructureParticleSystem>());
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Wait(async, IO.ReadAsync(async.File),
                wait =>
                {
                    List<AsyncLoadContent> asyncs = new List<AsyncLoadContent>();

                    ByteRefReader reader = new ByteRefReader(wait.Data);
                    reader.AddOnDeserialize(TEXTURE.Deserializer(Manager, asyncs));
                    reader.AddOnDeserialize(FONT.Deserializer(Manager, null));
                    TEXTURE result = new ParticleSystem(reader.ReadObject<StructureParticleSystem>());

                    // 等待异步加载的延迟图片加载完成
                    if (asyncs.Count == 0)
                        async.SetData(result);
                    else
                        Wait(async, asyncs, () => result);
                });
        }
    }
}

#endif