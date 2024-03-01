using System;
using System.Collections.Generic;

namespace Sandbox;

[GameResource("Outfit's Data", "outfits", "A collection of outfits", Icon = "clothing")]
public partial class OutfitData : GameResource
{
    [Property] public List<Clothing> Hats { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Hair { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Beard { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Skin { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Footwear { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Tops { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Gloves { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Facial { get; set; } = new List<Clothing>();
    [Property] public List<Clothing> Pants { get; set; } = new List<Clothing>();


    public static int RandomInt(int max) => Random.Shared.Int(0, max);

    public Clothing GetRandomHat() => Hats[RandomInt(Hats.Count - 1)];
    public Clothing GetRandomHair() => Hair[RandomInt(Hair.Count - 1)];
    public Clothing GetRandomSkin() => Skin[RandomInt(Skin.Count - 1)];
    public Clothing GetRandomFootwear() => Footwear[RandomInt(Footwear.Count - 1)];
    public Clothing GetRandomTop() => Tops[RandomInt(Tops.Count - 1)];
    public Clothing GetRandomGlove() => Gloves[RandomInt(Gloves.Count - 1)];
    public Clothing GetRandomFacial() => Facial[RandomInt(Facial.Count - 1)];
    public Clothing GetRandomBeard() => Beard[RandomInt(Beard.Count - 1)];
    public Clothing GetRandomPants() => Pants[RandomInt(Pants.Count - 1)];

    public List<Clothing> GetRandomOutfit()
    {
        List<Clothing> finalOutfit = new List<Clothing>();
        if (Hats.Count > 0) finalOutfit.Add(GetRandomHat());
        if (Hair.Count > 0) finalOutfit.Add(GetRandomHair());
        if (Skin.Count > 0) finalOutfit.Add(GetRandomSkin());
        if (Gloves.Count > 0) finalOutfit.Add(GetRandomGlove());
        if (Facial.Count > 0) finalOutfit.Add(GetRandomFacial());
        if (Beard.Count > 0) finalOutfit.Add(GetRandomBeard());
        if (Footwear.Count > 0) finalOutfit.Add(GetRandomFootwear());
        if (Tops.Count > 0) finalOutfit.Add(GetRandomTop());
        if (Pants.Count > 0) finalOutfit.Add(GetRandomPants());

        return finalOutfit;
    }
}