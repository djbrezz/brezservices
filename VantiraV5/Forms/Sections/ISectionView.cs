namespace VantiraV5.Forms.Sections;

internal interface ISectionView
{
    string SectionKey { get; }

    Task OnActivatedAsync();
}
