using Stem.Aspects;
using Utilities.Extensions;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;

namespace Stem.Rules;

public struct SimpleStringAppearance 
{
    public string value;

    public double glyphScale = 20, spacing = 25;

    public bool dirty = true, ui = false;

    public SimpleStringAppearance(string value)
    {
        this.value = value;
    }
}

public class SimpleTypesetterRule : Rule 
{
    protected override Archetype Archetype => Archetype.Create<Position2D, SimpleStringAppearance>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        for (int index = 0; index < entities.Length; ++index) 
        {
            var entity = entities[index];
            ref var appearance = ref store.GetAspectRef<SimpleStringAppearance>(entity);
            if (!appearance.dirty) continue;
            appearance.dirty = false;
            var (spacing, scale, ui) = (appearance.spacing, appearance.glyphScale, appearance.ui);
            var position = store.GetAspect<Position2D>(entity);
            List<int> glyphEntities;
            if (Book.IsSet<int, List<int>>(entity)) 
            {
                glyphEntities = Book.Get<int, List<int>>(entity);
            } else 
            {
                glyphEntities = new();
                Book.Set(entity, glyphEntities);
            }
            if (glyphEntities.Count > appearance.value.Length) {
                glyphEntities.Skip(appearance.value.Length).ForEach(glyphEntity => store.DeleteEntity(glyphEntity));
                glyphEntities.RemoveRange(appearance.value.Length, glyphEntities.Count - appearance.value.Length);
            } else if (glyphEntities.Count < appearance.value.Length) {
                var newEntities = Ints.ZeroUpUntil(appearance.value.Length - glyphEntities.Count).Select(_ => store.NewEntity()).ToList();
                newEntities.Indexed().ForEach(x => {
                    var (newEntity, i) = x;
                    store.Assign<Position2D>(newEntity, new() { X = position.X + spacing * (glyphEntities.Count + i), Y = position.Y });
                    if (ui) {
                        store.Assign<UIGlyphAppearance>(newEntity, new() { glyphIndex = '?', scale = (float) scale, depth = -1f });
                    } else {
                        store.Assign<GlyphAppearance>(newEntity, new() { glyphIndex = '?', scale = (float) scale });
                    }
                });
                glyphEntities.AddRange(newEntities);
            }
            int yOffset = 0;
            int xOffset = 0;
            for (int charIndex = 0; charIndex < appearance.value.Length; ++charIndex) {
                var glyphChar = appearance.value[charIndex];
                var glyphEntity = glyphEntities[charIndex];
                if (ui) {
                    ref var glyphApp = ref store.GetAspectRef<UIGlyphAppearance>(glyphEntity);
                    if (glyphChar == '\n') {
                        yOffset += 1;
                        xOffset = 0;
                        glyphApp.depth = 2f; // Behind the screen
                        continue;
                    }
                    glyphApp.glyphIndex = glyphChar;
                    glyphApp.scale = (float) scale;
                    glyphApp.depth = -1f;
                } else {
                    ref var glyphApp = ref store.GetAspectRef<GlyphAppearance>(glyphEntity);
                    if (glyphChar == '\n') {
                        yOffset += 1;
                        xOffset = 0;
                        glyphApp.depth = 2f; // Behind the screen
                        continue;
                    }
                    glyphApp.glyphIndex = glyphChar;
                    glyphApp.scale = (float) scale;
                    glyphApp.depth = -0.5f;
                }
                ref var glyphPos = ref store.GetAspectRef<Position2D>(glyphEntity);
                glyphPos.X = position.X + spacing * (xOffset);
                glyphPos.Y = position.Y - spacing * (2 * yOffset);
                xOffset += 1;
            }
        }
    }
}
