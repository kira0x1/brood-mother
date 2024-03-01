using System;
using System.Collections.Generic;
using System.Linq;

namespace Kira;

public struct Outfit
{
    public List<Clothing> Clothes { get; set; }

    public Outfit()
    {
        Clothes = new List<Clothing>();
    }
}

[Group("Kira")]
[Title("Clothing Renderer"), Icon("manage_accounts")]
public sealed class ClothingRenderer : Component
{
    [Property] private SkinnedModelRenderer Body { get; set; }
    [Property] private bool UseOutfitData { get; set; } = false;

    [Property, ShowIf(nameof(UseOutfitData), false)] private List<Outfit> Outfits { get; set; } = new List<Outfit>();
    [Property, ShowIf(nameof(UseOutfitData), true)] private OutfitData OutfitData { get; set; }

    [Property] public bool ApplyClothesOnStart { get; set; } = false;
    [Property] private bool PickRandomOutfit { get; set; } = true;

    [Property, ShowIf(nameof(PickRandomOutfit), false)]
    private int OutfitChosen { get; set; } = 0;


    protected override void OnStart()
    {
        base.OnStart();

        if (ApplyClothesOnStart && Body.IsValid())
        {
            ApplyClothing();
        }
    }

    public void SetOutfits(List<Outfit> outfits)
    {
        Outfits = outfits;
    }

    public void SetOutfitData(OutfitData data)
    {
        OutfitData = data;
    }

    public void ApplyClothing()
    {
        if (!Body.IsValid())
        {
            return;
        }

        ClothingContainer clothing = new ClothingContainer();
        var outfit = UseOutfitData ? OutfitData.GetRandomOutfit() : Outfits[PickRandomOutfit ? Random.Shared.Int(0, Outfits.Count - 1) : OutfitChosen].Clothes;
        clothing.Clothing = outfit.Where(c => c is not null).ToList();
        clothing.Apply(Body);
    }
}