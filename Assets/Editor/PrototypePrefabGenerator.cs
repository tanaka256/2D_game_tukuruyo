#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrototypePrefabGenerator
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string EnemyPrefabFolder = PrefabFolder + "/Enemies";
    private const string GimmickPrefabFolder = PrefabFolder + "/Gimmicks";
    private const string BackgroundPrefabFolder = PrefabFolder + "/Background";
    private const string MaterialFolder = "Assets/Materials";
    private const string TexturePath = PrefabFolder + "/PrototypeSquare.png";
    private const string CrouchingTexturePath = PrefabFolder + "/PrototypePlayerCrouching.png";
    private const string BackgroundTexturePath = PrefabFolder + "/PrototypeBackground.png";
    private const string AudioFolder = "Assets/Audio";
    private const string SfxFolder = AudioFolder + "/SFX";

    [MenuItem("Prototype/Generate Prototype Prefabs")]
    public static void Generate()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(EnemyPrefabFolder);
        EnsureFolder(GimmickPrefabFolder);
        EnsureFolder(BackgroundPrefabFolder);
        EnsureFolder(MaterialFolder);

        Sprite squareSprite = CreateSquareSprite();
        Sprite crouchingSprite = CreateSquareSprite(CrouchingTexturePath);
        Sprite backgroundSprite = CreateSquareSprite(BackgroundTexturePath);
        Material playerMaterial = CreateMaterial("PrototypePlayer", new Color(0.2f, 0.72f, 1f));
        Material platformMaterial = CreateMaterial("PrototypePlatform", new Color(0.33f, 0.38f, 0.43f));
        Material projectileMaterial = CreateMaterial("PrototypeProjectile", new Color(1f, 0.85f, 0.25f));

        string projectilePath = PrefabFolder + "/Projectile.prefab";
        string missilePath = PrefabFolder + "/Missile.prefab";
        string enemyProjectilePath = EnemyPrefabFolder + "/EnemyProjectile.prefab";
        string platformPath = PrefabFolder + "/Platform.prefab";
        string playerPath = PrefabFolder + "/Player.prefab";

        GameObject projectile = CreateProjectile(squareSprite, projectileMaterial);
        SavePrefab(projectile, projectilePath);
        Object.DestroyImmediate(projectile);

        GameObject missile = CreateProjectile(
            squareSprite,
            CreateMaterial("PrototypeMissile", new Color(0.4f, 1f, 1f)),
            PrototypeFaction.Player,
            6,
            new Vector3(0.55f, 0.2f, 1f));
        missile.GetComponent<SpriteRenderer>().color = new Color(0.4f, 1f, 1f);
        SavePrefab(missile, missilePath);
        Object.DestroyImmediate(missile);

        GameObject enemyProjectile = CreateProjectile(squareSprite, CreateMaterial("PrototypeEnemyProjectile", new Color(1f, 0.25f, 0.2f)), PrototypeFaction.Enemy, 2);
        SavePrefab(enemyProjectile, enemyProjectilePath);
        Object.DestroyImmediate(enemyProjectile);

        GameObject platform = CreatePlatform(squareSprite, platformMaterial);
        SavePrefab(platform, platformPath);
        Object.DestroyImmediate(platform);

        GameObject player = CreatePlayer(
            squareSprite,
            crouchingSprite,
            playerMaterial,
            AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath),
            AssetDatabase.LoadAssetAtPath<GameObject>(missilePath));
        SavePrefab(player, playerPath);
        Object.DestroyImmediate(player);

        CreateEnemyPrefabs(squareSprite, AssetDatabase.LoadAssetAtPath<GameObject>(enemyProjectilePath));
        CreateGimmickPrefabs(squareSprite);
        CreateBackgroundPrefab(backgroundSprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GenerateAndAssignSfx();
        Debug.Log("Prototype prefabs generated in Assets/Prefabs.");
    }

    [MenuItem("Prototype/Generate and Assign SFX")]
    public static void GenerateAndAssignSfx()
    {
        EnsureFolder(AudioFolder);
        EnsureFolder(SfxFolder);

        GenerateWave("PlayerShot", 0.12f, 1250f, 520f, 0.12f, 9f);
        GenerateWave("PlayerCharge", 0.42f, 420f, 1050f, 0.03f, 2.5f);
        GenerateWave("PlayerChargedShot", 0.32f, 980f, 210f, 0.08f, 7f);
        GenerateWave("PlayerMissile", 0.36f, 180f, 760f, 0.18f, 5f);
        GenerateWave("EnemyShot", 0.16f, 760f, 260f, 0.1f, 8f);
        GenerateWave("PlayerHit", 0.2f, 300f, 105f, 0.24f, 10f);
        GenerateWave("EnemyHit", 0.16f, 520f, 185f, 0.3f, 12f);
        GenerateWave("EnemyDeath", 0.48f, 470f, 70f, 0.42f, 6f);
        GenerateWave("GuardBlock", 0.19f, 1550f, 650f, 0.12f, 13f);
        GenerateWave("PlayerJump", 0.17f, 300f, 720f, 0.04f, 8f);
        GenerateWave("PlayerLand", 0.14f, 170f, 85f, 0.38f, 14f);

        AssetDatabase.Refresh();
        AssignAudioToPrefab(PrefabFolder + "/Player.prefab", true);

        string[] enemyGuids = AssetDatabase.FindAssets("t:Prefab", new[] { EnemyPrefabFolder });
        foreach (string guid in enemyGuids)
        {
            AssignAudioToPrefab(AssetDatabase.GUIDToAssetPath(guid), false);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated SFX in Assets/Audio/SFX and assigned clips to player/enemy prefabs.");
    }

    private static void GenerateWave(
        string clipName,
        float duration,
        float startFrequency,
        float endFrequency,
        float noiseMix,
        float decay)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        string path = SfxFolder + "/" + clipName + ".wav";
        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new(stream);

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + sampleCount * 2);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(sampleCount * 2);

        uint randomState = 0x9e3779b9u;
        double phase = 0.0;
        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)sampleRate;
            float progress = time / duration;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            phase += 2.0 * Mathf.PI * frequency / sampleRate;
            float tone = Mathf.Sin((float)phase);
            randomState ^= randomState << 13;
            randomState ^= randomState >> 17;
            randomState ^= randomState << 5;
            float noise = (randomState / (float)uint.MaxValue) * 2f - 1f;
            float attack = Mathf.Clamp01(time * 120f);
            float envelope = attack * Mathf.Exp(-decay * progress);
            float sample = (tone * (1f - noiseMix) + noise * noiseMix) * envelope * 0.72f;
            short pcm = (short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            writer.Write(pcm);
        }
    }

    private static void AssignAudioToPrefab(string prefabPath, bool isPlayer)
    {
        if (!File.Exists(prefabPath))
        {
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            PrototypeAudioEvents audioEvents = prefabRoot.GetComponent<PrototypeAudioEvents>();
            if (audioEvents == null)
            {
                audioEvents = prefabRoot.AddComponent<PrototypeAudioEvents>();
            }

            SerializedObject serializedAudio = new(audioEvents);
            SetClip(serializedAudio, "playerShot", "PlayerShot");
            SetClip(serializedAudio, "chargeStart", "PlayerCharge");
            SetClip(serializedAudio, "chargedShot", "PlayerChargedShot");
            SetClip(serializedAudio, "missileShot", "PlayerMissile");
            SetClip(serializedAudio, "playerHit", "PlayerHit");
            SetClip(serializedAudio, "playerJump", "PlayerJump");
            SetClip(serializedAudio, "playerLand", "PlayerLand");
            SetClip(serializedAudio, "enemyShot", "EnemyShot");
            SetClip(serializedAudio, "enemyHit", "EnemyHit");
            SetClip(serializedAudio, "enemyDeath", "EnemyDeath");
            SetClip(serializedAudio, "guardBlock", "GuardBlock");
            serializedAudio.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void SetClip(SerializedObject serializedObject, string propertyName, string clipName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SfxFolder + "/" + clipName + ".wav");
    }

    private static void CreateEnemyPrefabs(Sprite squareSprite, GameObject enemyProjectilePrefab)
    {
        CreateAndSaveEnemy(squareSprite, "PatrolEnemy", new Color(0.95f, 0.3f, 0.35f),
            EnemyPrefabFolder + "/PatrolEnemy.prefab", typeof(PrototypeEnemyPatrol), 3, 2.5f, 0f, 1);
        CreateAndSaveEnemy(squareSprite, "ChaserEnemy", new Color(0.8f, 0.25f, 0.8f),
            EnemyPrefabFolder + "/ChaserEnemy.prefab", typeof(PrototypeEnemyChaser), 4, 2.5f, 0f, 1);
        CreateAndSaveEnemy(squareSprite, "BouncerEnemy", new Color(1f, 0.5f, 0.15f),
            EnemyPrefabFolder + "/BouncerEnemy.prefab", typeof(PrototypeEnemyBouncer), 3, 2.5f, 8f, 1);
        CreateAndSaveEnemy(squareSprite, "FrontGuardPatrolEnemy", new Color(0.45f, 0.85f, 0.95f),
            EnemyPrefabFolder + "/FrontGuardPatrolEnemy.prefab", typeof(PrototypeEnemyPatrol), 4, 2.5f, 0f, 1, true);
        CreateAndSaveShootingEnemy(squareSprite, enemyProjectilePrefab, "TurretEnemy",
            new Color(0.95f, 0.25f, 0.7f), EnemyPrefabFolder + "/TurretEnemy.prefab", typeof(PrototypeEnemyTurret));
        CreateAndSaveShootingEnemy(squareSprite, enemyProjectilePrefab, "ApproachShooterEnemy",
            new Color(0.95f, 0.55f, 0.15f), EnemyPrefabFolder + "/ApproachShooterEnemy.prefab", typeof(PrototypeEnemyApproachShooter));
        CreateAndSaveShootingEnemy(squareSprite, enemyProjectilePrefab, "OmniTurretEnemy",
            new Color(0.25f, 0.85f, 0.95f), EnemyPrefabFolder + "/OmniTurretEnemy.prefab", typeof(PrototypeEnemyOmniTurret));
        CreateAndSaveShootingEnemy(squareSprite, enemyProjectilePrefab, "TrackingTurretEnemy",
            new Color(0.95f, 0.25f, 0.55f), EnemyPrefabFolder + "/TrackingTurretEnemy.prefab", typeof(PrototypeEnemyTrackingTurret));
    }

    private static void CreateGimmickPrefabs(Sprite squareSprite)
    {
        Material oneWayMaterial = CreateMaterial("PrototypeOneWayPlatform", new Color(0.25f, 0.65f, 0.8f));
        Material dropThroughMaterial = CreateMaterial("PrototypeDropThroughPlatform", new Color(0.2f, 0.85f, 0.55f));
        Material breakableMaterial = CreateMaterial("PrototypeBreakablePlatform", new Color(0.9f, 0.65f, 0.2f));
        Material movingMaterial = CreateMaterial("PrototypeMovingPlatform", new Color(0.55f, 0.45f, 0.85f));
        Material ladderMaterial = CreateMaterial("PrototypeLadder", new Color(0.85f, 0.55f, 0.25f));
        Material doorMaterial = CreateMaterial("PrototypeSceneDoor", new Color(0.2f, 0.75f, 0.95f));

        GameObject oneWay = CreateGimmickSquare("OneWayPlatform", squareSprite, oneWayMaterial, new Vector3(4f, 0.35f, 1f));
        AddOneWayComponents(oneWay);
        oneWay.AddComponent<PrototypeSidePassThrough>();
        SavePrefab(oneWay, GimmickPrefabFolder + "/OneWayPlatform.prefab");
        Object.DestroyImmediate(oneWay);

        GameObject dropThrough = CreateGimmickSquare("DropThroughPlatform", squareSprite, dropThroughMaterial, new Vector3(4f, 0.35f, 1f));
        AddOneWayComponents(dropThrough, false);
        dropThrough.AddComponent<PrototypeDropThroughPlatform>();
        dropThrough.AddComponent<PrototypeSidePassThrough>();
        SavePrefab(dropThrough, GimmickPrefabFolder + "/DropThroughPlatform.prefab");
        Object.DestroyImmediate(dropThrough);

        GameObject breakable = CreateGimmickSquare("BreakablePlatform", squareSprite, breakableMaterial, new Vector3(3f, 0.35f, 1f));
        BoxCollider2D breakableCollider = breakable.AddComponent<BoxCollider2D>();
        breakableCollider.usedByEffector = true;
        breakable.AddComponent<PlatformEffector2D>();
        breakable.AddComponent<PrototypeBreakablePlatform>();
        breakable.AddComponent<PrototypeSidePassThrough>();
        SavePrefab(breakable, GimmickPrefabFolder + "/BreakablePlatform.prefab");
        Object.DestroyImmediate(breakable);

        GameObject moving = CreateGimmickSquare("MovingPlatform", squareSprite, movingMaterial, new Vector3(3f, 0.35f, 1f));
        Rigidbody2D movingBody = moving.AddComponent<Rigidbody2D>();
        movingBody.bodyType = RigidbodyType2D.Kinematic;
        movingBody.useFullKinematicContacts = true;
        BoxCollider2D movingCollider = moving.AddComponent<BoxCollider2D>();
        movingCollider.usedByEffector = true;
        moving.AddComponent<PlatformEffector2D>();
        moving.AddComponent<PrototypeMovingPlatform>();
        moving.AddComponent<PrototypeSidePassThrough>();
        SavePrefab(moving, GimmickPrefabFolder + "/MovingPlatform.prefab");
        Object.DestroyImmediate(moving);

        GameObject ladder = CreateGimmickSquare("Ladder", squareSprite, ladderMaterial, new Vector3(0.7f, 4f, 1f));
        BoxCollider2D ladderCollider = ladder.AddComponent<BoxCollider2D>();
        ladderCollider.isTrigger = true;
        ladder.AddComponent<PrototypeLadder>();
        SavePrefab(ladder, GimmickPrefabFolder + "/Ladder.prefab");
        Object.DestroyImmediate(ladder);

        GameObject door = CreateGimmickSquare("SceneDoor", squareSprite, doorMaterial, new Vector3(1f, 2f, 1f));
        BoxCollider2D doorCollider = door.AddComponent<BoxCollider2D>();
        doorCollider.isTrigger = true;
        PrototypeSceneDoor doorScript = door.AddComponent<PrototypeSceneDoor>();

        GameObject arrivalPoint = new("ArrivalPoint");
        arrivalPoint.transform.SetParent(door.transform, false);
        arrivalPoint.transform.localPosition = new Vector3(0f, -1.25f, 0f);

        SerializedObject doorObject = new(doorScript);
        doorObject.FindProperty("doorId").stringValue = "Door";
        doorObject.FindProperty("destinationScene").stringValue = "";
        doorObject.FindProperty("destinationDoorId").stringValue = "";
        doorObject.FindProperty("fallbackSpawnPosition").vector3Value = Vector3.zero;
        doorObject.FindProperty("arrivalPoint").objectReferenceValue = arrivalPoint.transform;
        doorObject.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(door, GimmickPrefabFolder + "/SceneDoor.prefab");
        Object.DestroyImmediate(door);
    }

    private static void CreateBackgroundPrefab(Sprite backgroundSprite)
    {
        GameObject background = new("LoopingBackground");
        PrototypeLoopingBackground controller = background.AddComponent<PrototypeLoopingBackground>();
        SerializedObject controllerObject = new(controller);
        controllerObject.FindProperty("backgroundSprite").objectReferenceValue = backgroundSprite;
        controllerObject.FindProperty("tileSize").vector2Value = new Vector2(20f, 12f);
        controllerObject.FindProperty("loopHorizontal").boolValue = true;
        controllerObject.FindProperty("loopVertical").boolValue = false;
        controllerObject.FindProperty("followFactor").floatValue = 1f;
        controllerObject.FindProperty("sortingOrder").intValue = -100;
        controllerObject.ApplyModifiedPropertiesWithoutUndo();
        controller.BuildPreviewTiles();
        SavePrefab(background, BackgroundPrefabFolder + "/LoopingBackground.prefab");
        Object.DestroyImmediate(background);
    }

    private static GameObject CreateGimmickSquare(string objectName, Sprite sprite, Material material, Vector3 scale)
    {
        GameObject gimmick = CreateSquare(objectName, sprite, material, material.color);
        gimmick.transform.localScale = scale;
        return gimmick;
    }

    private static void AddOneWayComponents(GameObject platform, bool addBehaviour = true)
    {
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.usedByEffector = true;

        PlatformEffector2D effector = platform.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.surfaceArc = 180f;
        if (addBehaviour)
        {
            platform.AddComponent<PrototypeOneWayPlatform>();
        }
    }

    private static void CreateAndSaveEnemy(
        Sprite squareSprite,
        string objectName,
        Color color,
        string prefabPath,
        System.Type aiType,
        int healthValue,
        float moveSpeed,
        float bounceSpeed,
        int direction,
        bool addFrontDefense = false)
    {
        GameObject enemy = CreateSquare(objectName, squareSprite, CreateMaterial(objectName, color), color);

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        PrototypeHealth health = enemy.AddComponent<PrototypeHealth>();
        SerializedObject healthObject = new(health);
        healthObject.FindProperty("maxHealth").intValue = healthValue;
        healthObject.FindProperty("faction").enumValueIndex = (int)PrototypeFaction.Enemy;
        healthObject.FindProperty("playerInvincibilityDuration").floatValue = 0f;
        healthObject.ApplyModifiedPropertiesWithoutUndo();

        PrototypeContactDamage contactDamage = enemy.AddComponent<PrototypeContactDamage>();
        SerializedObject contactObject = new(contactDamage);
        contactObject.FindProperty("damage").intValue = 1;
        contactObject.ApplyModifiedPropertiesWithoutUndo();
        contactDamage.EnsureHitbox();

        if (addFrontDefense)
        {
            enemy.AddComponent<PrototypeFrontDefense>();
        }

        Component ai = enemy.AddComponent(aiType);
        SerializedObject aiObject = new(ai);
        SerializedProperty speedProperty = aiObject.FindProperty("moveSpeed");
        if (speedProperty != null)
        {
            speedProperty.floatValue = moveSpeed;
        }

        SerializedProperty directionProperty = aiObject.FindProperty("startDirection");
        if (directionProperty != null)
        {
            directionProperty.intValue = direction;
        }

        SerializedProperty bounceProperty = aiObject.FindProperty("bounceSpeed");
        if (bounceProperty != null)
        {
            bounceProperty.floatValue = bounceSpeed;
        }

        aiObject.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(enemy, prefabPath);
        Object.DestroyImmediate(enemy);
    }

    private static void CreateAndSaveShootingEnemy(
        Sprite squareSprite,
        GameObject enemyProjectilePrefab,
        string objectName,
        Color color,
        string prefabPath,
        System.Type shootingType)
    {
        GameObject enemy = CreateSquare(objectName, squareSprite, CreateMaterial(objectName, color), color);
        enemy.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        PrototypeHealth health = enemy.AddComponent<PrototypeHealth>();
        SerializedObject healthObject = new(health);
        healthObject.FindProperty("maxHealth").intValue = 4;
        healthObject.FindProperty("faction").enumValueIndex = (int)PrototypeFaction.Enemy;
        healthObject.FindProperty("playerInvincibilityDuration").floatValue = 0f;
        healthObject.ApplyModifiedPropertiesWithoutUndo();

        PrototypeContactDamage contactDamage = enemy.AddComponent<PrototypeContactDamage>();
        SerializedObject contactObject = new(contactDamage);
        contactObject.FindProperty("damage").intValue = 1;
        contactObject.ApplyModifiedPropertiesWithoutUndo();
        contactDamage.EnsureHitbox();

        PrototypeEnemyActivation activation = enemy.AddComponent<PrototypeEnemyActivation>();
        SerializedObject activationObject = new(activation);
        activationObject.FindProperty("requireVisibleBeforeStarting").boolValue = true;
        activationObject.FindProperty("viewportPadding").floatValue = 0.05f;
        activationObject.ApplyModifiedPropertiesWithoutUndo();

        PrototypeEnemyProjectileShooter shooter = enemy.AddComponent<PrototypeEnemyProjectileShooter>();
        GameObject spawnPoint = new("ProjectileSpawnPoint");
        spawnPoint.transform.SetParent(enemy.transform, false);
        spawnPoint.transform.localPosition = new Vector3(0.75f, 0f, 0f);
        SerializedObject shooterObject = new(shooter);
        shooterObject.FindProperty("projectilePrefab").objectReferenceValue = enemyProjectilePrefab;
        shooterObject.FindProperty("projectileSpawnPoint").objectReferenceValue = spawnPoint.transform;
        shooterObject.FindProperty("projectileSpeed").floatValue = 8f;
        shooterObject.FindProperty("projectileDamage").intValue = 1;
        shooterObject.ApplyModifiedPropertiesWithoutUndo();

        Component shootingBehaviour = enemy.AddComponent(shootingType);
        SerializedObject behaviourObject = new(shootingBehaviour);
        SerializedProperty interval = behaviourObject.FindProperty("shotInterval");
        if (interval != null)
        {
            interval.floatValue = shootingType == typeof(PrototypeEnemyApproachShooter)
                ? 1.1f
                : 1.25f;
        }

        SerializedProperty detectionRange = behaviourObject.FindProperty("detectionRange");
        if (detectionRange != null)
        {
            detectionRange.floatValue = 8f;
        }

        SerializedProperty muzzleDistance = behaviourObject.FindProperty("muzzleDistance");
        if (muzzleDistance != null)
        {
            muzzleDistance.floatValue = 0.8f;
        }

        behaviourObject.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(enemy, prefabPath);
        Object.DestroyImmediate(enemy);
    }

    private static GameObject CreatePlayer(
        Sprite squareSprite,
        Sprite crouchingSprite,
        Material material,
        GameObject projectilePrefab,
        GameObject missilePrefab)
    {
        GameObject player = CreateSquare("Player", squareSprite, material, new Color(0.2f, 0.72f, 1f));
        player.transform.localScale = new Vector3(0.8f, 1.4f, 1f);

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        PrototypePlayerController controller = player.AddComponent<PrototypePlayerController>();
        PrototypeHealth health = player.AddComponent<PrototypeHealth>();
        SerializedObject healthObject = new(health);
        healthObject.FindProperty("maxHealth").intValue = 5;
        healthObject.FindProperty("faction").enumValueIndex = (int)PrototypeFaction.Player;
        healthObject.FindProperty("playerInvincibilityDuration").floatValue = 1f;
        healthObject.ApplyModifiedPropertiesWithoutUndo();

        player.AddComponent<PrototypeHitFlash>();

        PrototypeCameraFollow cameraFollow = player.AddComponent<PrototypeCameraFollow>();
        cameraFollow.enabled = true;

        GameObject spawnPoint = new("ProjectileSpawnPoint");
        spawnPoint.transform.SetParent(player.transform);
        spawnPoint.transform.localPosition = new Vector3(0.58f, 0.08f, 0f);

        SerializedObject controllerObject = new(controller);
        controllerObject.FindProperty("crouchingSprite").objectReferenceValue = crouchingSprite;
        controllerObject.FindProperty("projectileSpawnPoint").objectReferenceValue = spawnPoint.transform;
        controllerObject.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        controllerObject.FindProperty("missilePrefab").objectReferenceValue = missilePrefab;
        controllerObject.FindProperty("chargedShotDamage").intValue = 4;
        controllerObject.FindProperty("chargedShotSpeed").floatValue = 16f;
        controllerObject.FindProperty("chargeTime").floatValue = 0.75f;
        controllerObject.FindProperty("missileDamage").intValue = 6;
        controllerObject.FindProperty("missileSpeed").floatValue = 10f;
        controllerObject.FindProperty("maxMissiles").intValue = 5;
        controllerObject.FindProperty("missileCount").intValue = 5;
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static GameObject CreatePlatform(Sprite squareSprite, Material material)
    {
        GameObject platform = CreateSquare("Platform", squareSprite, material, new Color(0.33f, 0.38f, 0.43f));
        platform.transform.localScale = new Vector3(4f, 0.5f, 1f);
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        platform.AddComponent<PrototypeSidePassThrough>();
        return platform;
    }

    private static GameObject CreateProjectile(
        Sprite squareSprite,
        Material material,
        PrototypeFaction faction = PrototypeFaction.Player,
        int damage = 1,
        Vector3? scale = null)
    {
        GameObject projectile = CreateSquare("Projectile", squareSprite, material, new Color(1f, 0.85f, 0.25f));
        projectile.transform.localScale = scale ?? new Vector3(0.28f, 0.12f, 1f);

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        PrototypeProjectile projectileScript = projectile.AddComponent<PrototypeProjectile>();
        SerializedObject projectileObject = new(projectileScript);
        projectileObject.FindProperty("damage").intValue = damage;
        projectileObject.FindProperty("faction").enumValueIndex = (int)faction;
        projectileObject.ApplyModifiedPropertiesWithoutUndo();
        return projectile;
    }

    private static GameObject CreateSquare(string objectName, Sprite squareSprite, Material material, Color color)
    {
        GameObject square = new(objectName);
        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.sharedMaterial = material;
        renderer.color = color;
        return square;
    }

    private static Sprite CreateSquareSprite()
    {
        return CreateSquareSprite(TexturePath);
    }

    private static Sprite CreateSquareSprite(string texturePath)
    {
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            // The prototype uses one Unity unit as one square. Unity's default
            // of 100 pixels per unit would make the generated 1x1 PNG 0.01 units.
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 1f))
            {
                importer.spritePixelsPerUnit = 1f;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        if (sprite == null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite importedSprite)
                {
                    sprite = importedSprite;
                    break;
                }
            }
        }

        if (sprite == null)
        {
            throw new System.InvalidOperationException(
                texturePath + " could not be imported as a Sprite.");
        }

        return sprite;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string materialPath = MaterialFolder + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SavePrefab(GameObject instance, string prefabPath)
    {
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parent = System.IO.Path.GetDirectoryName(folderPath).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
