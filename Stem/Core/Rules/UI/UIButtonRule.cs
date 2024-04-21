using System;
using Stem.Aspects;
using Stem.Rules.UI.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Stem.Rules.UI;

public struct UIButtonBehavior
{
    public string actionName;
}

public class UIButtonRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, UIButtonBehavior, UIRectAppearance>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var mouse = Global.Get<MouseState>();
        var window = Global.Get<StemWindow>();
        var mousePosition = mouse.Position;
        mousePosition.X -= window.ClientSize.X / 2f;
        mousePosition.Y -= window.ClientSize.Y / 2f;

        for (int index = 0; index < entities.Length; ++index)
        {
            var entity = entities[index];
            var position = store.GetAspect<Position2D>(entity);
            ref var appearance = ref store.GetAspectRef<UIRectAppearance>(entity);

            var withinLeft = mousePosition.X >= position.X - appearance.xScale / 2f;
            var withinRight = mousePosition.X <= position.X + appearance.xScale / 2f;
            var withinTop = -mousePosition.Y >= position.Y - appearance.yScale / 2f;
            var withinBottom = -mousePosition.Y <= position.Y + appearance.yScale / 2f;

            if (withinLeft && withinRight && withinTop && withinBottom)
            {
                if (mouse.IsButtonDown(MouseButton.Left) && !mouse.WasButtonDown(MouseButton.Left))
                {
                    var action = Global.Get<Action<IEntityStore, IState>>(store.GetAspect<UIButtonBehavior>(entity).actionName);
                    action(store, Global);
                }
                appearance.r = 1;
            } else
            {
                appearance.r = 0;
            }
        }
    }
}
