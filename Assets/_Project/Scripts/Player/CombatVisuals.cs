using UnityEngine;

// Sprites pequeños generados una sola vez; no dependen de texturas externas.
public static class CombatVisuals
{
    private static Sprite projectile;
    private static Sprite slash;

    public static Sprite Projectile
    {
        get
        {
            if (projectile == null) projectile = CreateSprite(false);
            return projectile;
        }
    }

    public static Sprite Slash
    {
        get
        {
            if (slash == null) slash = CreateSprite(true);
            return slash;
        }
    }

    private static Sprite CreateSprite(bool isSlash)
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = isSlash ? "Melee slash" : "Magic projectile";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - size / 2f) / (size / 2f);
                float dy = (y + 0.5f - size / 2f) / (size / 2f);
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha;
                if (isSlash)
                {
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    float arc = Mathf.Clamp01((95f - Mathf.Abs(angle)) / 15f);
                    alpha = Mathf.Clamp01((radius - 0.48f) / 0.08f) *
                            Mathf.Clamp01((0.96f - radius) / 0.08f) * arc;
                    pixels[y * size + x] = new Color(0.35f + (1f - radius) * 0.7f, 0.95f, 1f, alpha);
                }
                else
                {
                    alpha = Mathf.Clamp01((0.98f - radius) / 0.3f);
                    Color glow = Color.Lerp(new Color(0.05f, 0.4f, 1f), Color.white,
                        Mathf.Clamp01((0.55f - radius) / 0.35f));
                    glow.a = alpha;
                    pixels[y * size + x] = glow;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = texture.name;
        return sprite;
    }

#if UNITY_EDITOR
    public static void VerifyVisualAssets()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/Test/Template1.unity");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Attack attack = player != null ? player.GetComponent<Attack>() : null;
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;
        if (movement == null || movement.InputActions == null ||
            movement.InputActions.FindActionMap("Player", false) == null)
            throw new System.Exception("Falta el mapa Player del nuevo Input System.");

        var playerMap = movement.InputActions.FindActionMap("Player", true);
        foreach (string action in new[] { "Move", "Jump", "Dash", "Attack", "Throw" })
            if (playerMap.FindAction(action, false) == null)
                throw new System.Exception("Falta la acción de entrada: " + action);

        bool deadzone = false;
        foreach (var binding in playerMap.FindAction("Move", true).bindings)
        {
            if (binding.path == "<Joystick>/stick")
                throw new System.Exception("Move todavía escucha un joystick sin calibrar.");
            if (binding.path == "<Gamepad>/leftStick" && binding.processors.Contains("stickDeadzone"))
                deadzone = true;
        }
        if (!deadzone) throw new System.Exception("Al movimiento del mando le falta zona muerta.");

        if (attack == null || attack.throwableObject == null ||
            attack.throwableObject.GetComponent<ThrowableWeapon>() == null)
            throw new System.Exception("El jugador no tiene un proyectil utilizable.");
        if (attack.throwableObject.transform.localScale.x < 1f ||
            Object.FindAnyObjectByType<PlayerHUD>() == null)
            throw new System.Exception("Falta el HUD o el tamaño nuevo del proyectil.");

        AnimationClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_Project/Animations/Player/AttackGround.anim");
        if (clip == null) throw new System.Exception("No se ha encontrado el clip de ataque.");
        var bindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip);
        if (bindings.Length == 0) throw new System.Exception("El ataque no tiene fotogramas.");
        foreach (var binding in bindings)
        {
            foreach (var frame in UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding))
                if (!(frame.value is Sprite))
                    throw new System.Exception("El ataque tiene un sprite no encontrado en t=" + frame.time);
        }

        if (Projectile == null || Slash == null ||
            Projectile.texture.GetPixel(32, 32).a < 0.9f ||
            Physics2D.GetIgnoreLayerCollision(player.layer, 20) ||
            Physics2D.GetIgnoreLayerCollision(attack.throwableObject.layer, 9))
            throw new System.Exception("El proyectil, golpe o las capas de colisión no son válidos.");

        Ally ally = Object.FindAnyObjectByType<Ally>();
        if (ally == null || ally.throwableObject == null ||
            ally.throwableObject.GetComponent<ThrowableProjectile>() == null ||
            Physics2D.GetIgnoreLayerCollision(ally.throwableObject.layer, player.layer))
            throw new System.Exception("El proyectil enemigo no puede alcanzar al jugador.");

        Animator allyAnimator = ally.GetComponent<Animator>();
        if (allyAnimator == null || allyAnimator.runtimeAnimatorController == null ||
            ally.GetComponent<SpriteRenderer>().sprite == null ||
            ally.transform.Find("FallCheck") == null ||
            new UnityEditor.SerializedObject(ally).FindProperty("groundLayer").intValue == 0)
            throw new System.Exception("El enemigo perseguidor necesita sprites y detector de suelo.");

        foreach (AnimationClip enemyClip in allyAnimator.runtimeAnimatorController.animationClips)
        {
            if (enemyClip == null)
                throw new System.Exception("Hay un estado sin animación en PF_Ally_AI.");
            foreach (var binding in UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(enemyClip))
                foreach (var frame in UnityEditor.AnimationUtility.GetObjectReferenceCurve(enemyClip, binding))
                    if (!(frame.value is Sprite))
                        throw new System.Exception("Sprite perdido en la animación enemiga: " + enemyClip.name);
        }

        // Comprueba la detección real contra la geometría de Template1, no solo el rango serializado.
        GameObject target = new GameObject("Comprobación de visión", typeof(BoxCollider2D));
        GameObject previousTarget = ally.enemy;
        try
        {
            target.layer = player.layer;
            target.transform.position = ally.transform.position + new Vector3(2f, 0.5f, 0f);
            ally.enemy = target;
            Physics2D.SyncTransforms();
            var canSee = typeof(Ally).GetMethod("CanSeePlayer",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (canSee == null || !(bool)canSee.Invoke(ally, null))
                throw new System.Exception("El slime no ve un objetivo cercano y sin obstáculos.");

            target.transform.position = ally.transform.position + new Vector3(ally.rangeDist + 2f, 0f, 0f);
            Physics2D.SyncTransforms();
            if ((bool)canSee.Invoke(ally, null))
                throw new System.Exception("El slime ve objetivos fuera de su alcance.");
        }
        finally
        {
            ally.enemy = previousTarget;
            Object.DestroyImmediate(target);
        }

        GameObject boundary = GameObject.Find("LimiteInferior");
        if (boundary == null || boundary.GetComponent<KillZone>() == null ||
            boundary.GetComponent<BoxCollider2D>() == null ||
            !boundary.GetComponent<BoxCollider2D>().isTrigger ||
            Physics2D.GetIgnoreLayerCollision(boundary.layer, player.layer))
            throw new System.Exception("El límite inferior no puede detectar las caídas.");

        Debug.Log("CombatVisuals: escena, entradas, HUD, visión del slime, animaciones y colisiones verificados.");
    }
#endif
}
