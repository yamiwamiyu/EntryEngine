using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class EditorParticle
{
    public EntryEngine.UI.Panel PTool = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button BNew = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BOpen = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BSave = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextureBox TB161117153458 = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.Label L161117153551 = new EntryEngine.UI.Label();
    public EntryEngine.UI.TextBox TBDirectory = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.Button BHelp = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel P161129151337 = new EntryEngine.UI.Panel();
    public EntryEngine.UI.CheckBox CBPlay = new EntryEngine.UI.CheckBox();
    public EntryEngine.UI.Button BBack = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BForward = new EntryEngine.UI.Button();
    public EntryEngine.UI.TextureBox TBTimelineBottom = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextureBox TBTimeline = new EntryEngine.UI.TextureBox();
    public EntryEngine.UI.TextBox TBTime = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.TextBox TBDuration = new EntryEngine.UI.TextBox();
    public EntryEngine.UI.Label L161129154822 = new EntryEngine.UI.Label();
    public EntryEngine.UI.Panel PViewPreview = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Panel PViewProperty = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Panel PViewPF = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Panel PPFTool = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Button BUndo = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BRedo = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BMove = new EntryEngine.UI.Button();
    public EntryEngine.UI.Button BDelete = new EntryEngine.UI.Button();
    public EntryEngine.UI.Panel PViewDisplay = new EntryEngine.UI.Panel();
    public EntryEngine.UI.Label LOffset = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LScale = new EntryEngine.UI.Label();
    public EntryEngine.UI.Label LParticleCount = new EntryEngine.UI.Label();
    
    
    private void Initialize()
    {
        this.Name = "EditorParticle";
        EntryEngine.RECT this_Clip = new EntryEngine.RECT();
        this_Clip.X = 0f;
        this_Clip.Y = 0f;
        this_Clip.Width = 1280f;
        this_Clip.Height = 720f;
        this.Clip = this_Clip;
        
        this.Anchor = (EAnchor)15;
        this.Color = new COLOR()
        {
            B = 53,
            G = 53,
            R = 53,
            A = 255,
        };
        PTool.Name = "PTool";
        EntryEngine.RECT PTool_Clip = new EntryEngine.RECT();
        PTool_Clip.X = 0f;
        PTool_Clip.Y = 0f;
        PTool_Clip.Width = 1280f;
        PTool_Clip.Height = 32f;
        PTool.Clip = PTool_Clip;
        
        PTool.Anchor = (EAnchor)5;
        this.Add(PTool);
        BNew.Name = "BNew";
        EntryEngine.RECT BNew_Clip = new EntryEngine.RECT();
        BNew_Clip.X = 18f;
        BNew_Clip.Y = 0f;
        BNew_Clip.Width = 32f;
        BNew_Clip.Height = 32f;
        BNew.Clip = BNew_Clip;
        
        BNew.UIText = new EntryEngine.UI.UIText();
        BNew.UIText.Text = "";
        BNew.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BNew.UIText.TextAlignment = (EPivot)17;
        BNew.UIText.TextShader = null;
        BNew.UIText.Padding.X = 0f;
        BNew.UIText.Padding.Y = 0f;
        BNew.UIText.FontSize = 0f;
        PTool.Add(BNew);
        BOpen.Name = "BOpen";
        EntryEngine.RECT BOpen_Clip = new EntryEngine.RECT();
        BOpen_Clip.X = 50f;
        BOpen_Clip.Y = 0f;
        BOpen_Clip.Width = 32f;
        BOpen_Clip.Height = 32f;
        BOpen.Clip = BOpen_Clip;
        
        BOpen.UIText = new EntryEngine.UI.UIText();
        BOpen.UIText.Text = "";
        BOpen.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BOpen.UIText.TextAlignment = (EPivot)17;
        BOpen.UIText.TextShader = null;
        BOpen.UIText.Padding.X = 0f;
        BOpen.UIText.Padding.Y = 0f;
        BOpen.UIText.FontSize = 0f;
        PTool.Add(BOpen);
        BSave.Name = "BSave";
        EntryEngine.RECT BSave_Clip = new EntryEngine.RECT();
        BSave_Clip.X = 82f;
        BSave_Clip.Y = 0f;
        BSave_Clip.Width = 32f;
        BSave_Clip.Height = 32f;
        BSave.Clip = BSave_Clip;
        
        BSave.UIText = new EntryEngine.UI.UIText();
        BSave.UIText.Text = "";
        BSave.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BSave.UIText.TextAlignment = (EPivot)17;
        BSave.UIText.TextShader = null;
        BSave.UIText.Padding.X = 0f;
        BSave.UIText.Padding.Y = 0f;
        BSave.UIText.FontSize = 0f;
        PTool.Add(BSave);
        TB161117153458.Name = "TB161117153458";
        EntryEngine.RECT TB161117153458_Clip = new EntryEngine.RECT();
        TB161117153458_Clip.X = 161f;
        TB161117153458_Clip.Y = 0f;
        TB161117153458_Clip.Width = 32f;
        TB161117153458_Clip.Height = 32f;
        TB161117153458.Clip = TB161117153458_Clip;
        
        PTool.Add(TB161117153458);
        L161117153551.Name = "L161117153551";
        EntryEngine.RECT L161117153551_Clip = new EntryEngine.RECT();
        L161117153551_Clip.X = 192f;
        L161117153551_Clip.Y = 6.875f;
        L161117153551_Clip.Width = 64f;
        L161117153551_Clip.Height = 18.25f;
        L161117153551.Clip = L161117153551_Clip;
        
        L161117153551.UIText = new EntryEngine.UI.UIText();
        L161117153551.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L161117153551.UIText.TextAlignment = (EPivot)0;
        L161117153551.UIText.TextShader = null;
        L161117153551.UIText.Padding.X = 0f;
        L161117153551.UIText.Padding.Y = 0f;
        L161117153551.UIText.FontSize = 16f;
        PTool.Add(L161117153551);
        TBDirectory.Name = "TBDirectory";
        EntryEngine.RECT TBDirectory_Clip = new EntryEngine.RECT();
        TBDirectory_Clip.X = 259f;
        TBDirectory_Clip.Y = 2f;
        TBDirectory_Clip.Width = 975f;
        TBDirectory_Clip.Height = 29.125f;
        TBDirectory.Clip = TBDirectory_Clip;
        
        TBDirectory.Anchor = (EAnchor)7;
        TBDirectory.Color = new COLOR()
        {
            B = 116,
            G = 116,
            R = 116,
            A = 255,
        };
        TBDirectory.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBDirectory.DefaultText = new EntryEngine.UI.UIText();
        TBDirectory.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBDirectory.DefaultText.TextAlignment = (EPivot)16;
        TBDirectory.DefaultText.TextShader = null;
        TBDirectory.DefaultText.Padding.X = 20f;
        TBDirectory.DefaultText.Padding.Y = 0f;
        TBDirectory.DefaultText.FontSize = 16f;
        TBDirectory.UIText = new EntryEngine.UI.UIText();
        TBDirectory.UIText.Text = "";
        TBDirectory.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        TBDirectory.UIText.TextAlignment = (EPivot)16;
        TBDirectory.UIText.TextShader = new TextShader();
        TBDirectory.UIText.TextShader.Offset.X = 2f;
        TBDirectory.UIText.TextShader.Offset.Y = 2f;
        TBDirectory.UIText.TextShader.Color = new COLOR()
        {
            B = 128,
            G = 128,
            R = 128,
            A = 128,
        };
        TBDirectory.UIText.Padding.X = 20f;
        TBDirectory.UIText.Padding.Y = 0f;
        TBDirectory.UIText.FontSize = 16f;
        TBDirectory.CanSelect = false;
        PTool.Add(TBDirectory);
        BHelp.Name = "BHelp";
        EntryEngine.RECT BHelp_Clip = new EntryEngine.RECT();
        BHelp_Clip.X = 1245f;
        BHelp_Clip.Y = 0f;
        BHelp_Clip.Width = 32f;
        BHelp_Clip.Height = 32f;
        BHelp.Clip = BHelp_Clip;
        
        BHelp.Anchor = (EAnchor)6;
        BHelp.UIText = new EntryEngine.UI.UIText();
        BHelp.UIText.Text = "";
        BHelp.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BHelp.UIText.TextAlignment = (EPivot)17;
        BHelp.UIText.TextShader = null;
        BHelp.UIText.Padding.X = 0f;
        BHelp.UIText.Padding.Y = 0f;
        BHelp.UIText.FontSize = 16f;
        PTool.Add(BHelp);
        P161129151337.Name = "P161129151337";
        EntryEngine.RECT P161129151337_Clip = new EntryEngine.RECT();
        P161129151337_Clip.X = 640f;
        P161129151337_Clip.Y = 672f;
        P161129151337_Clip.Width = 640f;
        P161129151337_Clip.Height = 48f;
        P161129151337.Clip = P161129151337_Clip;
        
        P161129151337.Anchor = (EAnchor)29;
        this.Add(P161129151337);
        CBPlay.Name = "CBPlay";
        EntryEngine.RECT CBPlay_Clip = new EntryEngine.RECT();
        CBPlay_Clip.X = 9f;
        CBPlay_Clip.Y = 8.875f;
        CBPlay_Clip.Width = 32f;
        CBPlay_Clip.Height = 32f;
        CBPlay.Clip = CBPlay_Clip;
        
        CBPlay.UIText = new EntryEngine.UI.UIText();
        CBPlay.UIText.Text = "";
        CBPlay.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        CBPlay.UIText.TextAlignment = (EPivot)18;
        CBPlay.UIText.TextShader = null;
        CBPlay.UIText.Padding.X = 0f;
        CBPlay.UIText.Padding.Y = 0f;
        CBPlay.UIText.FontSize = 16f;
        P161129151337.Add(CBPlay);
        BBack.Name = "BBack";
        EntryEngine.RECT BBack_Clip = new EntryEngine.RECT();
        BBack_Clip.X = 47f;
        BBack_Clip.Y = 8.875f;
        BBack_Clip.Width = 32f;
        BBack_Clip.Height = 32f;
        BBack.Clip = BBack_Clip;
        
        BBack.UIText = new EntryEngine.UI.UIText();
        BBack.UIText.Text = "";
        BBack.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BBack.UIText.TextAlignment = (EPivot)17;
        BBack.UIText.TextShader = null;
        BBack.UIText.Padding.X = 0f;
        BBack.UIText.Padding.Y = 0f;
        BBack.UIText.FontSize = 16f;
        P161129151337.Add(BBack);
        BForward.Name = "BForward";
        EntryEngine.RECT BForward_Clip = new EntryEngine.RECT();
        BForward_Clip.X = 597f;
        BForward_Clip.Y = 8.875f;
        BForward_Clip.Width = 32f;
        BForward_Clip.Height = 32f;
        BForward.Clip = BForward_Clip;
        
        BForward.Anchor = (EAnchor)6;
        BForward.UIText = new EntryEngine.UI.UIText();
        BForward.UIText.Text = "";
        BForward.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BForward.UIText.TextAlignment = (EPivot)17;
        BForward.UIText.TextShader = null;
        BForward.UIText.Padding.X = 0f;
        BForward.UIText.Padding.Y = 0f;
        BForward.UIText.FontSize = 16f;
        P161129151337.Add(BForward);
        TBTimelineBottom.Name = "TBTimelineBottom";
        EntryEngine.RECT TBTimelineBottom_Clip = new EntryEngine.RECT();
        TBTimelineBottom_Clip.X = 82f;
        TBTimelineBottom_Clip.Y = 8.875f;
        TBTimelineBottom_Clip.Width = 512f;
        TBTimelineBottom_Clip.Height = 32f;
        TBTimelineBottom.Clip = TBTimelineBottom_Clip;
        
        TBTimelineBottom.Anchor = (EAnchor)5;
        P161129151337.Add(TBTimelineBottom);
        TBTimeline.Name = "TBTimeline";
        EntryEngine.RECT TBTimeline_Clip = new EntryEngine.RECT();
        TBTimeline_Clip.X = 82f;
        TBTimeline_Clip.Y = 8.875f;
        TBTimeline_Clip.Width = 512f;
        TBTimeline_Clip.Height = 32f;
        TBTimeline.Clip = TBTimeline_Clip;
        
        TBTimeline.Anchor = (EAnchor)5;
        P161129151337.Add(TBTimeline);
        TBTime.Name = "TBTime";
        EntryEngine.RECT TBTime_Clip = new EntryEngine.RECT();
        TBTime_Clip.X = 290f;
        TBTime_Clip.Y = 12.125f;
        TBTime_Clip.Width = 80f;
        TBTime_Clip.Height = 18.25f;
        TBTime.Clip = TBTime_Clip;
        
        TBTime.Anchor = (EAnchor)16;
        TBTime.Pivot = (EPivot)18;
        TBTime.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBTime.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBTime.DefaultText = new EntryEngine.UI.UIText();
        TBTime.DefaultText.Text = "";
        TBTime.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBTime.DefaultText.TextAlignment = (EPivot)0;
        TBTime.DefaultText.TextShader = null;
        TBTime.DefaultText.Padding.X = 0f;
        TBTime.DefaultText.Padding.Y = 0f;
        TBTime.DefaultText.FontSize = 16f;
        TBTime.UIText = new EntryEngine.UI.UIText();
        TBTime.UIText.Text = "#00:00.000";
        TBTime.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBTime.UIText.TextAlignment = (EPivot)17;
        TBTime.UIText.TextShader = null;
        TBTime.UIText.Padding.X = 0f;
        TBTime.UIText.Padding.Y = 0f;
        TBTime.UIText.FontSize = 16f;
        TBTime.Text = "#00:00.000";
        TBTime.CanSelect = false;
        TBTime.Readonly = true;
        P161129151337.Add(TBTime);
        TBDuration.Name = "TBDuration";
        EntryEngine.RECT TBDuration_Clip = new EntryEngine.RECT();
        TBDuration_Clip.X = 334f;
        TBDuration_Clip.Y = 11.125f;
        TBDuration_Clip.Width = 80f;
        TBDuration_Clip.Height = 18.25f;
        TBDuration.Clip = TBDuration_Clip;
        
        TBDuration.Anchor = (EAnchor)16;
        TBDuration.Pivot = (EPivot)16;
        TBDuration.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        TBDuration.CursorAreaColor = new COLOR()
        {
            B = 51,
            G = 153,
            R = 255,
            A = 255,
        };
        TBDuration.DefaultText = new EntryEngine.UI.UIText();
        TBDuration.DefaultText.Text = "";
        TBDuration.DefaultText.FontColor = new COLOR()
        {
            B = 211,
            G = 211,
            R = 211,
            A = 255,
        };
        TBDuration.DefaultText.TextAlignment = (EPivot)0;
        TBDuration.DefaultText.TextShader = null;
        TBDuration.DefaultText.Padding.X = 0f;
        TBDuration.DefaultText.Padding.Y = 0f;
        TBDuration.DefaultText.FontSize = 16f;
        TBDuration.UIText = new EntryEngine.UI.UIText();
        TBDuration.UIText.Text = "#00:00.000";
        TBDuration.UIText.FontColor = new COLOR()
        {
            B = 15,
            G = 86,
            R = 251,
            A = 255,
        };
        TBDuration.UIText.TextAlignment = (EPivot)17;
        TBDuration.UIText.TextShader = null;
        TBDuration.UIText.Padding.X = 0f;
        TBDuration.UIText.Padding.Y = 0f;
        TBDuration.UIText.FontSize = 16f;
        TBDuration.Text = "#00:00.000";
        P161129151337.Add(TBDuration);
        L161129154822.Name = "L161129154822";
        EntryEngine.RECT L161129154822_Clip = new EntryEngine.RECT();
        L161129154822_Clip.X = 312f;
        L161129154822_Clip.Y = 11.875f;
        L161129154822_Clip.Width = 8f;
        L161129154822_Clip.Height = 18.25f;
        L161129154822.Clip = L161129154822_Clip;
        
        L161129154822.Anchor = (EAnchor)16;
        L161129154822.Pivot = (EPivot)17;
        L161129154822.UIText = new EntryEngine.UI.UIText();
        L161129154822.UIText.FontColor = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        L161129154822.UIText.TextAlignment = (EPivot)0;
        L161129154822.UIText.TextShader = null;
        L161129154822.UIText.Padding.X = 0f;
        L161129154822.UIText.Padding.Y = 0f;
        L161129154822.UIText.FontSize = 16f;
        P161129151337.Add(L161129154822);
        PViewPreview.Name = "PViewPreview";
        EntryEngine.RECT PViewPreview_Clip = new EntryEngine.RECT();
        PViewPreview_Clip.X = 0f;
        PViewPreview_Clip.Y = 560f;
        PViewPreview_Clip.Width = 640f;
        PViewPreview_Clip.Height = 160f;
        PViewPreview.Clip = PViewPreview_Clip;
        
        PViewPreview.Anchor = (EAnchor)29;
        this.Add(PViewPreview);
        PViewProperty.Name = "PViewProperty";
        EntryEngine.RECT PViewProperty_Clip = new EntryEngine.RECT();
        PViewProperty_Clip.X = 460f;
        PViewProperty_Clip.Y = 32f;
        PViewProperty_Clip.Width = 180f;
        PViewProperty_Clip.Height = 528f;
        PViewProperty.Clip = PViewProperty_Clip;
        
        PViewProperty.Anchor = (EAnchor)31;
        this.Add(PViewProperty);
        PViewPF.Name = "PViewPF";
        EntryEngine.RECT PViewPF_Clip = new EntryEngine.RECT();
        PViewPF_Clip.X = 0f;
        PViewPF_Clip.Y = 32f;
        PViewPF_Clip.Width = 460f;
        PViewPF_Clip.Height = 528f;
        PViewPF.Clip = PViewPF_Clip;
        
        PViewPF.Anchor = (EAnchor)31;
        PViewPF.DragMode = (EDragMode)1;
        this.Add(PViewPF);
        PPFTool.Name = "PPFTool";
        EntryEngine.RECT PPFTool_Clip = new EntryEngine.RECT();
        PPFTool_Clip.X = 0f;
        PPFTool_Clip.Y = 521f;
        PPFTool_Clip.Width = 218f;
        PPFTool_Clip.Height = 39f;
        PPFTool.Clip = PPFTool_Clip;
        
        PPFTool.Anchor = (EAnchor)9;
        this.Add(PPFTool);
        BUndo.Name = "BUndo";
        EntryEngine.RECT BUndo_Clip = new EntryEngine.RECT();
        BUndo_Clip.X = 18f;
        BUndo_Clip.Y = 3.5f;
        BUndo_Clip.Width = 32f;
        BUndo_Clip.Height = 32f;
        BUndo.Clip = BUndo_Clip;
        
        BUndo.UIText = new EntryEngine.UI.UIText();
        BUndo.UIText.Text = "";
        BUndo.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BUndo.UIText.TextAlignment = (EPivot)17;
        BUndo.UIText.TextShader = null;
        BUndo.UIText.Padding.X = 0f;
        BUndo.UIText.Padding.Y = 0f;
        BUndo.UIText.FontSize = 16f;
        PPFTool.Add(BUndo);
        BRedo.Name = "BRedo";
        EntryEngine.RECT BRedo_Clip = new EntryEngine.RECT();
        BRedo_Clip.X = 50f;
        BRedo_Clip.Y = 3.5f;
        BRedo_Clip.Width = 32f;
        BRedo_Clip.Height = 32f;
        BRedo.Clip = BRedo_Clip;
        
        BRedo.UIText = new EntryEngine.UI.UIText();
        BRedo.UIText.Text = "";
        BRedo.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BRedo.UIText.TextAlignment = (EPivot)17;
        BRedo.UIText.TextShader = null;
        BRedo.UIText.Padding.X = 0f;
        BRedo.UIText.Padding.Y = 0f;
        BRedo.UIText.FontSize = 16f;
        PPFTool.Add(BRedo);
        BMove.Name = "BMove";
        EntryEngine.RECT BMove_Clip = new EntryEngine.RECT();
        BMove_Clip.X = 114f;
        BMove_Clip.Y = 3.5f;
        BMove_Clip.Width = 32f;
        BMove_Clip.Height = 32f;
        BMove.Clip = BMove_Clip;
        
        BMove.UIText = new EntryEngine.UI.UIText();
        BMove.UIText.Text = "";
        BMove.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BMove.UIText.TextAlignment = (EPivot)17;
        BMove.UIText.TextShader = null;
        BMove.UIText.Padding.X = 0f;
        BMove.UIText.Padding.Y = 0f;
        BMove.UIText.FontSize = 16f;
        PPFTool.Add(BMove);
        BDelete.Name = "BDelete";
        EntryEngine.RECT BDelete_Clip = new EntryEngine.RECT();
        BDelete_Clip.X = 146f;
        BDelete_Clip.Y = 3.5f;
        BDelete_Clip.Width = 32f;
        BDelete_Clip.Height = 32f;
        BDelete.Clip = BDelete_Clip;
        
        BDelete.UIText = new EntryEngine.UI.UIText();
        BDelete.UIText.Text = "";
        BDelete.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        BDelete.UIText.TextAlignment = (EPivot)17;
        BDelete.UIText.TextShader = null;
        BDelete.UIText.Padding.X = 0f;
        BDelete.UIText.Padding.Y = 0f;
        BDelete.UIText.FontSize = 16f;
        PPFTool.Add(BDelete);
        PViewDisplay.Name = "PViewDisplay";
        EntryEngine.RECT PViewDisplay_Clip = new EntryEngine.RECT();
        PViewDisplay_Clip.X = 640f;
        PViewDisplay_Clip.Y = 32f;
        PViewDisplay_Clip.Width = 640f;
        PViewDisplay_Clip.Height = 640f;
        PViewDisplay.Clip = PViewDisplay_Clip;
        
        PViewDisplay.Anchor = (EAnchor)31;
        PViewDisplay.Color = new COLOR()
        {
            B = 0,
            G = 0,
            R = 0,
            A = 255,
        };
        this.Add(PViewDisplay);
        LOffset.Name = "LOffset";
        EntryEngine.RECT LOffset_Clip = new EntryEngine.RECT();
        LOffset_Clip.X = 3f;
        LOffset_Clip.Y = 0f;
        LOffset_Clip.Width = 82f;
        LOffset_Clip.Height = 18.25f;
        LOffset.Clip = LOffset_Clip;
        
        LOffset.UIText = new EntryEngine.UI.UIText();
        LOffset.UIText.Text = "#100, 100";
        LOffset.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LOffset.UIText.TextAlignment = (EPivot)0;
        LOffset.UIText.TextShader = null;
        LOffset.UIText.Padding.X = 0f;
        LOffset.UIText.Padding.Y = 0f;
        LOffset.UIText.FontSize = 16f;
        LOffset.Text = "#100, 100";
        PViewDisplay.Add(LOffset);
        LScale.Name = "LScale";
        EntryEngine.RECT LScale_Clip = new EntryEngine.RECT();
        LScale_Clip.X = 3f;
        LScale_Clip.Y = 18.25f;
        LScale_Clip.Width = 40f;
        LScale_Clip.Height = 18.25f;
        LScale.Clip = LScale_Clip;
        
        LScale.UIText = new EntryEngine.UI.UIText();
        LScale.UIText.Text = "#1.0";
        LScale.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LScale.UIText.TextAlignment = (EPivot)0;
        LScale.UIText.TextShader = null;
        LScale.UIText.Padding.X = 0f;
        LScale.UIText.Padding.Y = 0f;
        LScale.UIText.FontSize = 16f;
        LScale.Text = "#1.0";
        PViewDisplay.Add(LScale);
        LParticleCount.Name = "LParticleCount";
        EntryEngine.RECT LParticleCount_Clip = new EntryEngine.RECT();
        LParticleCount_Clip.X = 3f;
        LParticleCount_Clip.Y = 36.5f;
        LParticleCount_Clip.Width = 48f;
        LParticleCount_Clip.Height = 18.25f;
        LParticleCount.Clip = LParticleCount_Clip;
        
        LParticleCount.UIText = new EntryEngine.UI.UIText();
        LParticleCount.UIText.Text = "#99999";
        LParticleCount.UIText.FontColor = new COLOR()
        {
            B = 255,
            G = 255,
            R = 255,
            A = 255,
        };
        LParticleCount.UIText.TextAlignment = (EPivot)0;
        LParticleCount.UIText.TextShader = null;
        LParticleCount.UIText.Padding.X = 0f;
        LParticleCount.UIText.Padding.Y = 0f;
        LParticleCount.UIText.FontSize = 16f;
        LParticleCount.Text = "#99999";
        PViewDisplay.Add(LParticleCount);
        
        this.PhaseShowing += Show;
    }
    protected override IEnumerable<ICoroutine> Loading()
    {
        ICoroutine ___async;
        PTool.Background = TEXTURE.Pixel;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"New.jpg", ___c => BNew.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Open.jpg", ___c => BOpen.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Save.jpg", ___c => BSave.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Config.jpg", ___c => TB161117153458.Texture = ___c));
        if (___async != null) yield return ___async;
        
        
        TBDirectory.SourceNormal = TEXTURE.Pixel;
        
        
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Help.jpg", ___c => BHelp.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        P161129151337.Background = PATCH.GetNinePatch(PATCH.NullColor, new COLOR(0, 0, 0, 255), 2);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Play.jpg", ___c => CBPlay.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Pause.jpg", ___c => CBPlay.SourceClicked = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Back.jpg", ___c => BBack.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Forward.jpg", ___c => BForward.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Timeline.jpg", ___c => TBTimelineBottom.Texture = ___c));
        if (___async != null) yield return ___async;
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"TimelineLight.jpg", ___c => TBTimeline.Texture = ___c));
        if (___async != null) yield return ___async;
        
        
        
        
        
        
        
        
        PViewPreview.Background = PATCH.GetNinePatch(PATCH.NullColor, new COLOR(0, 0, 0, 255), 2);
        PViewProperty.Background = PATCH.GetNinePatch(PATCH.NullColor, new COLOR(0, 0, 0, 255), 2);
        PViewPF.Background = PATCH.GetNinePatch(PATCH.NullColor, new COLOR(0, 0, 0, 255), 2);
        PPFTool.Background = PATCH.GetNinePatch(PATCH.NullColor, new COLOR(0, 0, 0, 255), 2);
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Undo.jpg", ___c => BUndo.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Redo.jpg", ___c => BRedo.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Move.jpg", ___c => BMove.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        ___async = LoadAsync(Content.LoadAsync<EntryEngine.TEXTURE>(@"Delete.jpg", ___c => BDelete.SourceNormal = ___c));
        if (___async != null) yield return ___async;
        
        PViewDisplay.Background = TEXTURE.Pixel;
        
        
        
        
        
        
        
        var __loading = MyLoading();
        if (__loading != null)
        foreach (var item in __loading)
        yield return item;
    }
    private void Show(UIScene __scene)
    {
        L161117153551.UIText.Text = _LANGUAGE.GetString("19");
        L161117153551.Text = _LANGUAGE.GetString("19");
        TBDirectory.DefaultText.Text = _LANGUAGE.GetString("20");
        L161129154822.UIText.Text = _LANGUAGE.GetString("32");
        L161129154822.Text = _LANGUAGE.GetString("32");
        
    }
}
