#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.Serialize;

namespace EntryEngine
{
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

        /// <summary>
        /// 更新粒子表现
        /// </summary>
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
    [AReflexible]public class PBByTime : ParticleStream
    {
        private float _remain;
        public int BornPerSecond = 60;
        public ushort LimitCount;
        private float _time;
        public float StartTime;
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
    [AReflexible]public class PSLifecycle : PSRandomSkip
    {
        public float Lifecycle = 2f;
        public float VaryP = 0.25f;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Life = Lifecycle;
            if (VaryP != 0)
                p.Life *= 1 + Random.NextSign() * Random.Next(VaryP);
            return true;
        }
    }
    [AReflexible]public class PSTex : PSSkip
    {
        public TEXTURE Texture = TEXTURE.Pixel;
        public COLOR Color = COLOR.White;
        public VECTOR2 Origin;
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
    [AReflexible]public class PSTexClone : PSTex
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            base.Update(p, ps, elapsed);
            p.Texture = (TEXTURE)Texture.Cache();
            return true;
        }
    }
    [AReflexible]public class PSPosPoint : PSSkip
    {
        public VECTOR2 Position;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Position.X = Position.X;
            p.Position.Y = Position.Y;
            return true;
        }
    }
    [AReflexible]public class PSPosRectangle : PSRandomSkip
    {
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
    [AReflexible]public class PSPosCircle : PSRandomSkip
    {
        public CIRCLE Area = new CIRCLE(50);
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            float radian = Random.NextRadian();
            float r = Random.Next(Area.R);
            CIRCLE.ParametricEquation(ref Area.C, r, radian, out p.Position);
            return true;
        }
    }
    [AReflexible]public class PSPosAbsolute : PSSkip
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.PosMode = 1;
            return true;
        }
    }
    [AReflexible]public class PSSpeed : PSRandomSkip
    {
        /// <summary>每秒移动的像素</summary>
        public float Speed = 300;
        public float VaryP;
        public float Direction;
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
    [AReflexible]public class PSScale : PSRandomSkip
    {
        public float Scale = 1;
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
    [AReflexible]public class PSRotation : PSRandomSkip
    {
        public float Rotation;
        public float VaryV = 180;
        public float Rotate;
        public float VaryRotateV;
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
        None = 0,
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        //All = 15,
    }
    [AReflexible]public class PSColor : PSRandomSkip
    {
        public COLOR From = COLOR.Black;
        public COLOR To = COLOR.White;
        public EPSColor Same = EPSColor.None;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            bool r = (Same & EPSColor.R) != EPSColor.None;
            bool g = (Same & EPSColor.G) != EPSColor.None;
            bool b = (Same & EPSColor.B) != EPSColor.None;
            bool a = (Same & EPSColor.A) != EPSColor.None;

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
    [AReflexible]public class PSProbability : PSRandom
    {
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
    [AReflexible]public class PSAnd : ParticleStream
    {
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
    [AReflexible]public class PSOr : ParticleStream
    {
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
    [AReflexible]public class PSInArea : ParticleStream
    {
        public RECT Area = new RECT(-50, -50, 100, 100);
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            return Area.Contains(p.Position.X, p.Position.Y) != Not;
        }
    }
    [AReflexible]public class PSInCircle : ParticleStream
    {
        public CIRCLE Area = new CIRCLE(50);
        public bool Not;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            return Area.Contains(p.Position.X, p.Position.Y) != Not;
        }
    }
    [AReflexible]public enum EPSCheck : byte
    {
        Euqals,
        Greater,
        GreaterEuqal,
        Less,
        LessEuqal,
    }
    [AReflexible]public class PSSpeedCheck : ParticleStream
    {
        /// <summary>每秒移动的像素</summary>
        public float Speed;
        public EPSCheck OP;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            switch (OP)
            {
                case EPSCheck.Euqals: return p.speed == Speed;
                case EPSCheck.Greater: return p.speed > Speed;
                case EPSCheck.GreaterEuqal: return p.speed >= Speed;
                case EPSCheck.Less: return p.speed < Speed;
                case EPSCheck.LessEuqal: return p.speed <= Speed;
                default: return true;
            }
        }
    }
    [AReflexible]public class PSTimer : ParticleStream
    {
        //private float _time;
        public float StartTime;
        public float Interval = -1;
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

    // MOTION
    [AReflexible]public class PSLifeAdd : ParticleStream
    {
        public float Lifecycle;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Life += Lifecycle;
            return true;
        }
    }
    [AReflexible]public abstract class PSPropertyAdd : PSRandom
    {
        public float VaryFrom;
        public float Vary;
        public float VaryPFrom;
        public float VaryP;
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
    [AReflexible]public class PSRotateAdd : PSPropertyAdd
    {
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Rotate += VaryValue(p.Rotate);
            return true;
        }
    }
    [AReflexible]public class PSRotationForward : ParticleStream
    {
        public float Offset;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Rotation = _MATH.ToRadian(p.direction + Offset);
            return true;
        }
    }
    [AReflexible]public class PSColorAdd : PSPropertyAdd
    {
        public EPSColor Change = EPSColor.A;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            if (VaryP == 0 && VaryPFrom == 0)
            {
                float vary = VaryValue(0);
                if (vary != 0)
                {
                    if ((Change & EPSColor.R) != EPSColor.None)
                        p.Color.R = _MATH.InByte(p.Color.R + vary);
                    if ((Change & EPSColor.G) != EPSColor.None)
                        p.Color.G = _MATH.InByte(p.Color.G + vary);
                    if ((Change & EPSColor.B) != EPSColor.None)
                        p.Color.B = _MATH.InByte(p.Color.B + vary);
                    if ((Change & EPSColor.A) != EPSColor.None)
                        p.Color.A = _MATH.InByte(p.Color.A + vary);
                }
            }
            else
            {
                if ((Change & EPSColor.R) != EPSColor.None)
                    p.Color.R = _MATH.InByte(p.Color.R + VaryValue(p.Color.R));
                if ((Change & EPSColor.G) != EPSColor.None)
                    p.Color.G = _MATH.InByte(p.Color.G + VaryValue(p.Color.G));
                if ((Change & EPSColor.B) != EPSColor.None)
                    p.Color.B = _MATH.InByte(p.Color.B + VaryValue(p.Color.B));
                if ((Change & EPSColor.A) != EPSColor.None)
                    p.Color.A = _MATH.InByte(p.Color.A + VaryValue(p.Color.A));
            }
            return true;
        }
    }

    [AReflexible]public class PSWind : ParticleStream
    {
        public VECTOR2 Force;
        public override bool Update(Particle p, ParticleEmitter ps, float elapsed)
        {
            p.Position.X += Force.X * elapsed;
            p.Position.Y += Force.Y * elapsed;
            return true;
        }
    }

    // DISAPPEAR
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
        public TEXTURE Texture;
        public VECTOR2 Position;
        public COLOR Color;
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
        // 绝对坐标，不会跟随粒子系统位移，但会跟随粒子系统的其它矩阵变化
        public VECTOR2 AbsolutePosition;
        // 0: 相对坐标 1: 绝对坐标尚未确定坐标 2: 绝对坐标已确定坐标
        public byte PosMode;
        public float Rotate;

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
            if (elapsed != 0)
                for (int i = 0; i < emitters.Count; i++)
                    emitters[i].Update(elapsed);
            _updated = true;
            _elapsed += elapsed;
        }
        public override void Update(GameTime time)
        {
            Update(time.ElapsedSecond);
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
        protected internal override Content Cache()
        {
            var cache = new ParticleSystem();
            cache.Duration = this.Duration;
            //cache.emitters = this.emitters;
            for (int i = 0; i < emitters.Count; i++)
                cache.emitters.Add((ParticleEmitter)emitters[i].Cache());
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
    [AReflexible]public class ParticleEmitter : TEXTURE, IUpdatable
    {
        private static Particle nullParticle = new Particle();

        private Pool<Particle> particles = new Pool<Particle>();
        private int _updated;
        private float _elapsed;
        internal int _stream;
        public List<ParticleStream> Flow;

        public int Count
        {
            get { return particles.Count; }
        }
        public override int Width
        {
            get { return 1; }
        }
        public override int Height
        {
            get { return 1; }
        }
        public override bool IsDisposed
        {
            get { return particles == null; }
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
            if (Flow == null || Flow.Count == 0 || elapsed == 0)
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
                int end = Flow.Count;
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
            p.Rotation += p.Rotate;

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
                if (sprite.Texture != null)
                {
                    if (sprite.PosMode == 1)
                    {
                        sprite.AbsolutePosition = apos;
                        sprite.PosMode = 2;
                    }
                    if (sprite.PosMode == 0)
                        graphics.Draw(sprite.Texture, sprite.Position, GRAPHICS.NullSource, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Flip);
                    else
                    {
                        VECTOR2 pos;
                        pos.X = sprite.Position.X + sprite.AbsolutePosition.X - apos.X;
                        pos.Y = sprite.Position.Y + sprite.AbsolutePosition.Y - apos.Y;
                        graphics.Draw(sprite.Texture, pos, GRAPHICS.NullSource, sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale, sprite.Flip);
                    }
                }
            });
        }
        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (_updated != GameTime.Time.FrameID)
            {
                _updated = GameTime.Time.FrameID;
                Update(GameTime.Time.ElapsedSecond);
            }

            if (particles.Count > 0)
            {
                MATRIX2x3 matrix;
                __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
                graphics.BeginFromPrevious(matrix);
                VECTOR2 apos = matrix.Translation;
                Draw(graphics, apos);
                graphics.End();
            }

            return true;
        }
        public void Reset()
        {
            _updated = 0;
            particles.ClearToFree();
            if (Flow != null)
                for (int i = 0; i < Flow.Count; i++)
                    if (Flow[i] != null)
                        Flow[i].Reset();
        }

        protected internal override Content Cache()
        {
            var cache = new ParticleEmitter();
            cache._Key = this._Key;
            if (this.Flow != null)
            {
                cache.Flow = new List<ParticleStream>();
                int count = this.Flow.Count;
                for (int i = 0; i < count; i++)
                {
                    cache.Flow.Add(this.Flow[i].Clone());
                }
            }
            return cache;
        }
        protected internal override void InternalDispose()
        {
            particles = null;
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
            reader.OnDeserialize = TEXTURE.Deserializer(Manager, null);
            return new ParticleSystem(reader.ReadObject<StructureParticleSystem>());
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Wait(async, IO.ReadAsync(async.File),
                wait =>
                {
                    List<AsyncLoadContent> asyncs = new List<AsyncLoadContent>();

                    ByteRefReader reader = new ByteRefReader(wait.Data);
                    reader.OnDeserialize = TEXTURE.Deserializer(Manager, asyncs);
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