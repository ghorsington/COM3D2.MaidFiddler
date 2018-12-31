// CM3D2-01さんの下記のコードをもとに改変しています
//     CM3D2：SystemShortcutにボタン追加
//     https://gist.github.com/CM3D2-01/adcf5072ff5ba812858a
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityInjector.Attributes;

namespace GearMenu
{
    /// <summary>
    /// 歯車メニューへのアイコン登録
    /// </summary>
    public static class Buttons
    {
        // 識別名の実体。同じ名前を保持すること。詳細は SetAndCallOnReposition を参照
        static string Name_ = "CM3D2.GearMenu.Buttons";

        // バージョン文字列の実体。改善、改造した場合は文字列の辞書順がより大きい値に更新すること
        static string Version_ = Name_ + " 0.0.2.0";

        /// <summary>
        /// 識別名
        /// </summary>
        public static string Name { get { return Name_; } }

        /// <summary>
        /// バージョン文字列
        /// </summary>
        public static string Version { get { return Version_; } }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(UnityInjector.PluginBase plugin, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, plugin, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, UnityInjector.PluginBase plugin, byte[] pngData, Action<GameObject> action)
        {
            var pluginNameAttr = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
            var pluginVersionAttr = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
            string pluginName = (pluginNameAttr == null) ? plugin.Name : pluginNameAttr.Name;
            string pluginVersion = (pluginVersionAttr == null) ? string.Empty : pluginVersionAttr.Version;
            string label = string.Format("{0} {1}", pluginName, pluginVersion);
            return Add(name, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add("テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string label, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, "テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, string label, byte[] pngData, Action<GameObject> action)
        {
            GameObject goButton = null;

            // 既に存在する場合は削除して続行
            if (Contains(name))
            {
                Remove(name);
            }

            if (action == null)
            {
                return goButton;
            }

            try
            {
                // ギアメニューの子として、コンフィグパネル呼び出しボタンを複製
                goButton = NGUITools.AddChild(Grid, UTY.GetChildObject(Grid, "Config", true));

                // 名前を設定
                if (name != null)
                {
                    goButton.name = name;
                }

                // イベントハンドラ設定（同時に、元から持っていたハンドラは削除）
                EventDelegate.Set(goButton.GetComponent<UIButton>().onClick, () => { action(goButton); });

                // ポップアップテキストを追加
                {
                    UIEventTrigger t = goButton.GetComponent<UIEventTrigger>();
                    EventDelegate.Add(t.onHoverOut, () => { SysShortcut.VisibleExplanation(null, false); });
                    EventDelegate.Add(t.onDragStart, () => { SysShortcut.VisibleExplanation(null, false); });
                    SetText(goButton, label);
                }

                // PNG イメージを設定
                {
                    if (pngData == null)
                    {
                        pngData = DefaultIcon.Png;
                    }

                    // 本当はスプライトを削除したいが、削除するとパネルのα値とのTween同期が動作しない
                    // (動作させる方法が分からない) ので、スプライトを描画しないように設定する
                    // もともと持っていたスプライトを削除する場合は以下のコードを使うと良い
                    //     NGUITools.Destroy(goButton.GetComponent<UISprite>());
                    UISprite us = goButton.GetComponent<UISprite>();
                    us.type = UIBasicSprite.Type.Filled;
                    us.fillAmount = 0.0f;

                    // テクスチャを生成
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(pngData);

                    // 新しくテクスチャスプライトを追加
                    UITexture ut = NGUITools.AddWidget<UITexture>(goButton);
                    ut.material = new Material(ut.shader);
                    ut.material.mainTexture = tex;
                    ut.MakePixelPerfect();
                }

                // グリッド内のボタンを再配置
                Reposition();
            }
            catch
            {
                // 既にオブジェクトを作っていた場合は削除
                if (goButton != null)
                {
                    NGUITools.Destroy(goButton);
                    goButton = null;
                }
                throw;
            }
            return goButton;
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void Remove(string name)
        {
            Remove(Find(name));
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static void Remove(GameObject go)
        {
            NGUITools.Destroy(go);
            Reposition();
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static bool Contains(string name)
        {
            return Find(name) != null;
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static bool Contains(GameObject go)
        {
            return Contains(go.name);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(string name, Color color)
        {
            SetFrameColor(Find(name), color);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(GameObject go, Color color)
        {
            var uiTexture = go.GetComponentInChildren<UITexture>();
            if (uiTexture == null)
            {
                return;
            }
            var tex = uiTexture.mainTexture as Texture2D;
            if (tex == null)
            {
                return;
            }
            for (int x = 1; x < tex.width - 1; x++)
            {
                tex.SetPixel(x, 0, color);
                tex.SetPixel(x, tex.height - 1, color);
            }
            for (int y = 1; y < tex.height - 1; y++)
            {
                tex.SetPixel(0, y, color);
                tex.SetPixel(tex.width - 1, y, color);
            }
            tex.Apply();
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void ResetFrameColor(string name)
        {
            ResetFrameColor(Find(name));
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        public static void ResetFrameColor(GameObject go)
        {
            SetFrameColor(go, DefaultFrameColor);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(string name, string label)
        {
            SetText(Find(name), label);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(GameObject go, string label)
        {
            var t = go.GetComponent<UIEventTrigger>();
            t.onHoverOver.Clear();
            EventDelegate.Add(t.onHoverOver, () => { SysShortcut.VisibleExplanation(label, label != null); });
            var b = go.GetComponent<UIButton>();

            // 既にホバー中なら説明を変更する
            if (b.state == UIButtonColor.State.Hover)
            {
                SysShortcut.VisibleExplanation(label, label != null);
            }
        }

        // システムショートカット内のGameObjectを見つける
        static GameObject Find(string name)
        {
            Transform t = GridUI.GetChildList().FirstOrDefault(c => c.gameObject.name == name);
            return t == null ? null : t.gameObject;
        }

        // グリッド内のボタンを再配置
        static void Reposition()
        {
            // 必要なら UIGrid.onRepositionを設定、呼び出しを行う
            SetAndCallOnReposition(GridUI);

            // 次回の UIGrid.Update 処理時にグリッド内のボタン再配置が行われるようリクエスト
            GridUI.repositionNow = true;
        }

        // 必要に応じて UIGrid.onReposition を登録、呼び出す
        static void SetAndCallOnReposition(UIGrid uiGrid)
        {
            string targetVersion = GetOnRepositionVersion(uiGrid);

            // バージョン文字列が null の場合、知らないクラスが登録済みなのであきらめる
            if (targetVersion == null)
            {
                return;
            }

            // 何も登録されていないか、自分より古いバージョンだったら新しい onReposition を登録する
            if (targetVersion == string.Empty || string.Compare(targetVersion, Version, false) < 0)
            {
                uiGrid.onReposition = (new OnRepositionHandler(Version)).OnReposition;
            }

            // PreOnReposition を持つ場合はそれを呼び出す
            if (uiGrid.onReposition != null)
            {
                object target = uiGrid.onReposition.Target;
                if (target != null)
                {
                    Type type = target.GetType();
                    MethodInfo mi = type.GetMethod("PreOnReposition");
                    if (mi != null)
                    {
                        mi.Invoke(target, new object[] { });
                    }
                }
            }
        }

        // UIGrid.onReposition を保持するオブジェクトのバージョン文字列を得る
        //  null            知らないクラスもしくはバージョン文字列だった
        //  string.Empty    UIGrid.onRepositionが未登録だった
        //  その他          取得したバージョン文字列
        static string GetOnRepositionVersion(UIGrid uiGrid)
        {
            if (uiGrid.onReposition == null)
            {
                // 未登録だった
                return string.Empty;
            }

            object target = uiGrid.onReposition.Target;
            if (target == null)
            {
                // Delegate.Target が null ということは、
                // UIGrid.onReposition は static なメソッドなので、たぶん知らないクラス
                return null;
            }

            Type type = target.GetType();
            if (type == null)
            {
                // 型情報が取れないので、あきらめる
                return null;
            }

            FieldInfo fi = type.GetField("Version", BindingFlags.Instance | BindingFlags.Public);
            if (fi == null)
            {
                // public な Version メンバーを持っていないので、たぶん知らないクラス
                return null;
            }

            string targetVersion = fi.GetValue(target) as string;
            if (targetVersion == null || !targetVersion.StartsWith(Name))
            {
                // 知らないバージョン文字列だった
                return null;
            }

            return targetVersion;
        }

        public static SystemShortcut SysShortcut { get { return GameMain.Instance.SysShortcut; } }
        public static UIPanel SysShortcutPanel { get { return SysShortcut.GetComponent<UIPanel>(); } }
        public static UISprite SysShortcutExplanation
        {
            get
            {
                Type type = typeof(SystemShortcut);
                FieldInfo fi = type.GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null)
                {
                    return null;
                }
                return fi.GetValue(SysShortcut) as UISprite;
            }
        }
        public static GameObject Base { get { return SysShortcut.gameObject.transform.Find("Base").gameObject; } }
        public static UISprite BaseSprite { get { return Base.GetComponent<UISprite>(); } }
        public static GameObject Grid { get { return Base.gameObject.transform.Find("Grid").gameObject; } }
        public static UIGrid GridUI { get { return Grid.GetComponent<UIGrid>(); } }
        public static readonly Color DefaultFrameColor = new Color(1f, 1f, 1f, 0f);

        // UIGrid.onReposition処理用のクラス
        // Delegate.Targetの値を生かすために、static ではなくインスタンスとして生成
        class OnRepositionHandler
        {
            public string Version;

            public OnRepositionHandler(string version)
            {
                this.Version = version;
            }

            public void OnReposition()
            {
            }

            public void PreOnReposition()
            {
                var g = GridUI;
                var b = BaseSprite;

                // ratio : 画面横幅に対するボタン全体の横幅の比率。0.5 なら画面半分
                float ratio = 3.0f / 4.0f;
                float pixelSizeAdjustment = UIRoot.GetPixelSizeAdjustment(Base);

                // g.cellWidth  = 39;
                g.cellHeight = g.cellWidth;
                g.arrangement = UIGrid.Arrangement.CellSnap;
                g.sorting = UIGrid.Sorting.None;
                g.pivot = UIWidget.Pivot.TopRight;
                g.maxPerLine = (int)(Screen.width / (g.cellWidth / pixelSizeAdjustment) * ratio);

                var children = g.GetChildList();
                int itemCount = children.Count;
                int spriteItemX = Math.Min(g.maxPerLine, itemCount);
                int spriteItemY = Math.Max(1, (itemCount - 1) / g.maxPerLine + 1);
                int spriteWidthMargin = (int)(g.cellWidth * 3 / 2 + 8);
                int spriteHeightMargin = (int)(g.cellHeight / 2);
                float pivotOffsetY = spriteHeightMargin * 1.5f + 1f;

                b.pivot = UIWidget.Pivot.TopRight;
                b.width = (int)(spriteWidthMargin + g.cellWidth * spriteItemX);
                b.height = (int)(spriteHeightMargin + g.cellHeight * spriteItemY + 2f);

                // (946,502) はもとの Base の localPosition の値
                // 他のオブジェクトから値を取れないだろうか？
                Base.transform.localPosition = new Vector3(946.0f, 502.0f + pivotOffsetY, 0.0f);

                // ここでの、高さ(spriteItemY)に応じて横方向に補正する意味が分からない。
                // たぶん何かを誤解している
                Grid.transform.localPosition = new Vector3(
                    -2.0f + (-spriteItemX - 1 + spriteItemY - 1) * g.cellWidth,
                    -1.0f - pivotOffsetY,
                    0f);

                {
                    int a = 0;
                    string[] specialNames = GameMain.Instance.CMSystem.NetUse ? OnlineButtonNames : OfflineButtonNames;
                    foreach (Transform child in children)
                    {
                        int i = a++;

                        // システムが持っているオブジェクトの場合は特別に順番をつける
                        int si = Array.IndexOf(specialNames, child.gameObject.name);
                        if (si >= 0)
                        {
                            i = si;
                        }

                        float x = (-i % g.maxPerLine + spriteItemX - 1) * g.cellWidth;
                        float y = (i / g.maxPerLine) * g.cellHeight;
                        child.localPosition = new Vector3(x, -y, 0f);
                    }
                }

                // マウスオーバー時のテキストの位置を指定
                {
                    UISprite sse = SysShortcutExplanation;
                    Vector3 v = sse.gameObject.transform.localPosition;
                    v.y = Base.transform.localPosition.y - b.height - sse.height;
                    sse.gameObject.transform.localPosition = v;
                }
            }

            // オンライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OnlineButtonNames = new string[] {
                "Config", "Ss", "SsUi", "Shop", "ToTitle", "Info", "Exit"
            };

            // オフライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OfflineButtonNames = new string[] {
                "Config", "Ss", "SsUi", "ToTitle", "Info", "Exit"
            };
        }
    }

    // デフォルトアイコン
    internal static class DefaultIcon
    {
        public static byte[] Png
        {
            get
            {
                if (png == null)
                {
                    png = Convert.FromBase64String(pngBase64);
                }
                return png;
            }
        }

        static byte[] png = null;

        // 32x32 ピクセルの PNG データを Base64 エンコードしたもの
        static string pngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAA3NCSVQICAjb4U/g"+
            "AAAACXBIWXMAABYlAAAWJQFJUiTwAAAA/0lEQVRIie2WPYqFMBRGb35QiARM4QZS"+
            "uAX3X7sDkWwgRYSQgJLEKfLGh6+bZywG/JrbnZPLJfChfd/hzuBb6QBA89i2zTln"+
            "jFmWZV1XAPjrZgghAKjrum1bIUTTNFVVvQXOOaXUNE0xxhDC9++llBDS972U8iTQ"+
            "Ws/zPAyDlPJreo5SahxHzrkQAo4baK0B4Dr9gGTgW4Ax5pxfp+dwzjH+JefhvaeU"+
            "lhJQSr33J0GMsRT9A3j7P3gEj+ARPIJHUFBACCnLPYAvAWPsSpn4SAiBMXYSpJSs"+
            "taUE1tqU0knQdR0AKKWu0zMkAwEA5QZnjClevHIvegnuq47o37frH81sg91rI7H3"+
            "AAAAAElFTkSuQmCC";
    }
}
