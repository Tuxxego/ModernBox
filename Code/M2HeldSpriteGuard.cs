using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // The game only loads held-item sprites for its own pool weapons. Any item without one
    // crashes actor rendering when a unit holds it (e.g. old saves with MIRVs equipped).
    internal static class M2HeldSpriteGuard
    {
        private static Sprite _blank;

        internal static void Apply()
        {
            int fixedCount = 0;
            foreach (EquipmentAsset item in AssetManager.items.list)
            {
                if (item == null || !IsM2Item(item.id)) continue;
                if ((item.gameplay_sprites != null && item.gameplay_sprites.Length > 0 && item.gameplay_sprites[0] != null)) continue;
                Sprite[] sprites = string.IsNullOrEmpty(item.path_gameplay_sprite) ? null : SpriteTextureLoader.getSpriteList(item.path_gameplay_sprite);
                if (sprites == null || sprites.Length == 0) sprites = new[] { Blank() };
                item.gameplay_sprites = sprites;
                // Held items are drawn from the item atlas, so the sprite has to be added there too.
                foreach (Sprite sprite in sprites)
                    if (sprite != null) DynamicSprites.preloadItemSprite(sprite, null);
                fixedCount++;
            }
            if (fixedCount > 0) ModernBoxDiagnostics.Info("Gave " + fixedCount + " items a held sprite.");
        }

        private static bool IsM2Item(string id)
        {
            return ModernBoxCatalog.EquipmentIds.Contains(id) || id.StartsWith(EquipmentAndTraitsRegistry.AttackPrefix, System.StringComparison.Ordinal) ||
                   OriginalM2Projectiles.AttackProjectiles.ContainsKey(id) ||
                   System.Array.IndexOf(ModernBoxCatalog.MirvIds, id) >= 0;
        }

        private static Sprite Blank()
        {
            if (_blank != null) return _blank;
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            texture.Apply();
            _blank = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _blank;
        }
    }
}
