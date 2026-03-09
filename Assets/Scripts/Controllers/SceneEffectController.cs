using Genoverrei.DesignPattern;
using NaughtyAttributes;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class SceneEffectController : Singleton<SceneEffectController>
{
    [Header("Components")]
    [SerializeField] private Animator _animator;

    [SerializeField] private AnimationClip _nextSceneEffect;

    public Coroutine SceneEffectCoroutine;


    private void OnValidate()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// สั่งเล่น Effect โดยใช้ชื่อ Trigger ใน Animator
    /// </summary>
    /// <param name="triggerName">ชื่อ Trigger เช่น "FadeIn", "FadeOut", "GameOver"</param>
    public void PlayEffect(string triggerName)
    {
        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            return;
        }

        _animator.SetTrigger(triggerName);
        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {triggerName}");
    }

    public void LoadSceneAndPlayEffect(string SceneName)
    {
        SceneEffectCoroutine ??= StartCoroutine(LoadSceneAndPlayEffectRotine(SceneName));
    }

    public void LoadSceneAndPlayEffect(int SceneBuildIndex)
    {
        SceneEffectCoroutine ??= StartCoroutine(LoadSceneAndPlayEffectRotine(SceneBuildIndex));
    }

    public void QuitGameAfterPlayEffect()
    {
        SceneEffectCoroutine ??= StartCoroutine(QuitGameRotine());
    }

    public IEnumerator LoadSceneAndPlayEffectRotine(string SceneName)
    {
        if (SceneEffectCoroutine != null) yield break;

        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            yield break;
        }

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");
        
        yield return new WaitForSeconds(_nextSceneEffect.length);

        SceneManager.LoadScene(SceneName);

        yield return new WaitForSeconds(_nextSceneEffect.length);

        EventBus.Instance.Publish(new LoadSceneEvent(false));
        SceneEffectCoroutine = null;
    }

    public IEnumerator LoadSceneAndPlayEffectRotine(int SceneBuildIndex)
    {
        if (SceneEffectCoroutine != null)
        {
            Debug.LogWarning("<r>[SceneEffect]</r> have Duplicate Corotine");
            yield break;
        }
        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            yield break;
        }

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");

        yield return new WaitForSeconds(_nextSceneEffect.length);

        SceneManager.LoadScene(SceneBuildIndex);

        yield return new WaitForSeconds(_nextSceneEffect.length);

        EventBus.Instance.Publish(new LoadSceneEvent(false));
        SceneEffectCoroutine = null;
    }

    private IEnumerator QuitGameRotine()
    {
        if (SceneEffectCoroutine != null)
        {
            Debug.LogWarning("<r>[SceneEffect]</r> have Duplicate Corotine");
            yield break;
        }

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");

        yield return new WaitForSeconds(_nextSceneEffect.length);

        Application.Quit();
    }

    public void PlayEffect(AnimationClip triggerName)
    {
        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            return;
        }

        _animator.Play(triggerName.name);
        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {triggerName}");
    }
}