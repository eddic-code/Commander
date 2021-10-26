// ReSharper disable InconsistentNaming

using System;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.PubSubSystem;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting;

namespace Commander.Utility
{
    public static class HighlightToggle
    {
        [HarmonyPatch(typeof(InteractionHighlightController), "HighlightOn")]
        private class InteractionHighlightControllerActivatePatch
        {
            private static AccessTools.FieldRef<InteractionHighlightController, bool> m_IsHighlightingRef;

            private static bool Prepare() 
            {
                m_IsHighlightingRef = Accessors.CreateFieldRef<InteractionHighlightController, bool>("m_IsHighlighting");
                return true;
            }

            private static bool Prefix(InteractionHighlightController __instance, bool ___m_Inactive) 
            {
                try 
                {
                    if (!Main.Enabled) { return true; }

                    if (m_IsHighlightingRef(__instance) & !___m_Inactive) 
                    {
                        m_IsHighlightingRef(__instance) = false;

                        foreach (var mapObjectEntityData in Game.Instance.State.MapObjects) 
                        {
                            mapObjectEntityData.View.UpdateHighlight();
                        }

                        foreach (var unitEntityData in Game.Instance.State.Units) 
                        {
                            unitEntityData.View.UpdateHighlight(false);
                        }

                        EventBus.RaiseEvent(delegate (IInteractionHighlightUIHandler h) 
                        {
                            h.HandleHighlightChange(false);
                        });

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Main.Error(ex.Message);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InteractionHighlightController), "HighlightOff")]
        private class InteractionHighlightControllerDeactivatePatch 
        {
            private static bool Prefix(InteractionHighlightController __instance) 
            {
                try
                {
                    return !Main.Enabled;
                }
                catch (Exception ex) 
                {
                    Main.Error(ex.Message);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MapObjectView), "UpdateHighlight")]
        internal class HighlightHiddenObjects 
        {
            private const string ObjName = "ToyBox.HiddenHighlighter";
            private const string DecalName = "ToyBox.DecalHiddenHighlighter";
            private static readonly Color HighlightColor0 = new(1.0f, 0.0f, 1.0f, 0.8f);
            private static readonly Color HighlightColor1 = new(0.0f, 0.0f, 1.0f, 1.0f);

            private static void Postfix(MapObjectView __instance) 
            {
                var data = __instance.Data;
                if (data == null) { return; }

                var pcc = __instance.GetComponent<PerceptionCheckComponent>();
                if (pcc != null && !data.IsPerceptionCheckPassed || !data.IsRevealed) 
                {
                    var is_highlighting = Game.Instance?.InteractionHighlightController?.IsHighlighting;

                    if (is_highlighting ?? false) 
                    {
                        HighlightOn(__instance);
                    }
                    else
                    {
                        HighlightOff(__instance);
                    }
                }
                else
                {
                    HighlightDestroy(__instance);
                }
            }

            private static GameObject HighlightCreate(Component view) 
            {
                var obj = new GameObject(ObjName);
                obj.transform.parent = view.transform;
                var highlighter = obj.AddComponent<Highlighter>();

                foreach (var polygon in view.transform.GetComponentsInChildren<ScriptZonePolygon>()) 
                {
                    var mesh = polygon.DecalMeshObject;
                    if (mesh == null) continue;

                    var renderer = mesh.GetComponent<MeshRenderer>();
                    if (renderer == null) continue;

                    var decal = UnityEngine.Object.Instantiate(renderer.gameObject, renderer.transform.parent);
                    decal.name = DecalName;

                    var decal_renderer = decal.GetComponent<MeshRenderer>();
                    decal_renderer.enabled = false;
                    decal_renderer.forceRenderingOff = true;
                }

                foreach (var renderer in view.transform.GetComponentsInChildren<Renderer>()) 
                {
                    highlighter.AddExtraRenderer(renderer);
                }

                return obj;
            }

            private static void HighlightDestroy(Component view) 
            {
                var decal = view?.transform?.Find(DecalName)?.gameObject;

                if (decal != null) 
                {
                    UnityEngine.Object.Destroy(decal);
                }

                var obj = view?.transform?.Find(ObjName)?.gameObject;

                if (obj != null) 
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }

            private static void HighlightOn(Component view) 
            {
                var obj = view.transform.Find(ObjName)?.gameObject;
                if (obj == null) { obj = HighlightCreate(view); }

                var highlighter = obj?.GetComponent<Highlighter>();
                if (highlighter == null) { return; }

                highlighter.ConstantOnImmediate(HighlightColor0);
                highlighter.FlashingOn(HighlightColor0, HighlightColor1, 1.0f);

                var decal = view.transform?.Find(DecalName)?.gameObject;
                if (decal == null) return;

                var renderer = decal.GetComponent<MeshRenderer>();
                if (renderer == null) return;

                renderer.enabled = true;
                renderer.forceRenderingOff = true;
            }

            private static void HighlightOff(Component view) 
            {
                var obj = view.transform.Find(ObjName)?.gameObject;
                if (obj == null) return;

                var highlighter = obj.GetComponent<Highlighter>();
                if (highlighter == null) { return; }

                highlighter.ConstantOff(0.0f);
                highlighter.FlashingOff();

                var decal = view.transform?.Find(DecalName)?.gameObject;
                if (decal == null) return;

                var renderer = decal.GetComponent<MeshRenderer>();
                if (renderer == null) return;

                renderer.enabled = false;
                renderer.forceRenderingOff = true;
            }
        }
    }
}
