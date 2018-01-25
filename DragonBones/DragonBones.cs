using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonBones;
using System.IO;
using EntryEngine.Serialize;

namespace EntryEngine.DragonBones
{
    public class DRAGON_BONES : TEXTURE, IDrawableTexture
    {
        internal ArmatureEE Armature;

        public override int Width
        {
            get { return (int)Armature._armatureData.aabb.width; }
        }
        public override int Height
        {
            get { return (int)Armature._armatureData.aabb.height; }
        }
        public override bool IsEnd
        {
            get
            {
                return base.IsEnd;
            }
        }
        public override bool IsDisposed
        {
            get { return false; }
        }

        public override void Update(GameTime time)
        {
            Armature.AdvanceTime(time.Elapsed);
        }
        void IDrawableTexture.Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            MATRIX2x3 matrix;
            __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
            graphics.Begin(matrix, null, true);

            var slots = Armature.GetSlots();
            SpriteVertex[] vertices = new SpriteVertex[slots.Sum(s => s._meshData.vertices.Count * 4)];

            graphics.End();
        }
        protected override void InternalDispose()
        {
        }
    }
    public class PipelineDragonBones : ContentPipeline
    {
        class FactoryEE : BaseFactory
        {
            internal PipelineDragonBones Pipeline;
            private EventDispatcherEE dispatcher = new EventDispatcherEE();

            public FactoryEE(PipelineDragonBones pipeline)
            {
                this.Pipeline = pipeline;
            }

            protected override TextureAtlasData _generateTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
            {
                if (textureAtlasData != null)
                {
                    //(textureAtlasData as TextureAtlasDataEE).Texture = textureAtlas as TEXTURE;
                }
                else
                {
                    textureAtlasData = new TextureAtlasDataEE();
                }

                return textureAtlasData;
            }
            protected override Armature _generateArmature(BuildArmaturePackage dataPackage)
            {
                ArmatureEE armature = new ArmatureEE();
                armature._init(
                    dataPackage.armature, dataPackage.skin,
                    null, null, dispatcher
                );
                //armature.animation.Play();
                return armature;
            }
            protected override Slot _generateSlot(BuildArmaturePackage dataPackage, SkinSlotData skinSlotData, Armature armature)
            {
                var slotData = skinSlotData.slot;
                var slot = BaseObject.BorrowObject<SlotEE>();

                slot._init(
                    skinSlotData,
                    null,
                    null);

                for (int i = 0, l = skinSlotData.displays.Count; i < l; ++i)
                {
                    var displayData = skinSlotData.displays[i];
                    switch (displayData.type)
                    {
                        case DisplayType.Image:
                        case DisplayType.Mesh:
                            if (displayData.texture == null)
                            {
                                displayData.texture = _getTextureData(dataPackage.dataName, displayData.path);
                            }

                            if (!string.IsNullOrEmpty(dataPackage.textureAtlasName))
                            {
                                slot._textureDatas[i] = _getTextureData(dataPackage.textureAtlasName, displayData.path);
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                return slot;
            }
        }

        private FactoryEE factory;
        public override IEnumerable<string> SuffixProcessable
        {
            get { yield return "db"; }
        }

        public PipelineDragonBones()
        {
            factory = new FactoryEE(this);
        }

        protected override Content Load(string file)
        {
            string temp = file.Substring(0, file.LastIndexOf('.'));
            string bone = IO.ReadText(temp + "_ske.json");
            var dbData = factory.ParseDragonBonesData(new JsonReader(bone).ReadDictionary());
            string atlas = IO.ReadText(temp + "_tex.json");
            TextureAtlasDataEE atlasData = (TextureAtlasDataEE)factory.ParseTextureAtlasData(new JsonReader(atlas).ReadDictionary(), null);
            atlasData.Texture = Manager.Load<TEXTURE>(temp + "_tex.png");

            Armature armature = factory.BuildArmature(dbData.armatureNames[0], dbData.name, null);

            DRAGON_BONES animation = new DRAGON_BONES();
            animation.Armature = (ArmatureEE)armature;
            return animation;
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            throw new NotImplementedException();
        }
    }

    public class EventDispatcherEE : IEventDispatcher<EventObject>
    {
        private readonly Dictionary<string, List<ListenerDelegate<EventObject>>> _listeners = new Dictionary<string, List<ListenerDelegate<EventObject>>>();

        /**
         * @private
         */
        virtual public void _onClear()
        {
            _listeners.Clear();
        }
        public void DispatchEvent(string type, EventObject eventObject)
        {
            if (!_listeners.ContainsKey(type))
            {
                return;
            }
            else
            {
                foreach (var item in _listeners[type])
                    item(type, eventObject);
            }
        }
        public bool HasEventListener(string type)
        {
            return _listeners.ContainsKey(type);
        }
        public void AddEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            List<ListenerDelegate<EventObject>> delegates;
            if (_listeners.ContainsKey(type))
            {
                delegates = _listeners[type];
                for (int i = 0, l = delegates.Count; i < l; ++i)
                {
                    if (listener == delegates[i])
                    {
                        return;
                    }
                }
            }
            else
            {
                delegates = new List<ListenerDelegate<EventObject>>();
                _listeners.Add(type, delegates);
            }
            delegates.Add(listener);
        }
        public void RemoveEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            List<ListenerDelegate<EventObject>> delegates;
            if (!_listeners.TryGetValue(type, out delegates))
                return;

            if (delegates.Remove(listener))
                if (delegates.Count == 0)
                    _listeners.Remove(type);
        }
    }
    public class ArmatureEE : Armature
    {
    }
    public class SlotEE : Slot
    {
        public TextureVertex Vertex;

        protected override void _initDisplay(object value)
        {
        }
        protected override void _disposeDisplay(object value)
        {
        }
        protected override void _onUpdateDisplay()
        {
        }
        protected override void _addDisplay()
        {
        }
        protected override void _replaceDisplay(object value)
        {
        }
        protected override void _removeDisplay()
        {
        }
        protected override void _updateZOrder()
        {
        }
        internal override void _updateVisible()
        {
        }
        protected override void _updateBlendMode()
        {
        }
        protected override void _updateColor()
        {
        }
        protected override void _updateFrame()
        {
        }
        protected override void _updateMesh()
        {
        }
        protected override void _updateTransform(bool isSkinnedMesh)
        {
        }
    }
    public class TextureAtlasDataEE : TextureAtlasData
    {
        public TEXTURE Texture;

        public override TextureData GenerateTextureData()
        {
            return new TextureDataEE(this);
        }
        protected override void _onClear()
        {
            base._onClear();

            Texture = null;
        }
    }
    public class TextureDataEE : TextureData
    {
        internal TextureAtlasDataEE Texture
        {
            get;
            private set;
        }
        internal TextureDataEE(TextureAtlasDataEE tex)
        {
            this.Texture = tex;
        }
    }
}
