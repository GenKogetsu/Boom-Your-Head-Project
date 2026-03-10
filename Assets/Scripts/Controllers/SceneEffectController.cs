using Genoverrei.DesignPattern;
using NaughtyAttributes;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class SceneEffectController : Singleton<SceneEffectController>
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip _nextSceneEffect;

    public Coroutine SceneEffectCoroutine;
    public bool HaveSceneEffectCoroutine;

    private void OnValidate()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }

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
        SceneEffectCoroutine = StartCoroutine(LoadSceneAndPlayEffectRoutine(SceneName));
    }

    public void LoadSceneAndPlayEffect(int SceneBuildIndex)
    {
        SceneEffectCoroutine = StartCoroutine(LoadSceneAndPlayEffectRoutine(SceneBuildIndex));
    }

    public void QuitGameAfterPlayEffect()
    {
        SceneEffectCoroutine = StartCoroutine(QuitGameRoutine());
    }

    public IEnumerator LoadSceneAndPlayEffectRoutine(string SceneName)
    {
        if (SceneEffectCoroutine != null) StopCoroutine(SceneEffectCoroutine);

        HaveSceneEffectCoroutine = true;

        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            yield break;
        }

        DisableAllInput();

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");

        yield return new WaitForSeconds(_nextSceneEffect.length);

        ReleaseAllPoolObjects();

        SceneManager.LoadScene(SceneName);

        yield return new WaitForSeconds(_nextSceneEffect.length);

        EventBus.Instance.Publish(new LoadSceneEvent(false));

        EnableAllInput();

        SceneEffectCoroutine = null;
        HaveSceneEffectCoroutine = false;
    }

    public IEnumerator LoadSceneAndPlayEffectRoutine(int SceneBuildIndex)
    {
        if (SceneEffectCoroutine != null) StopCoroutine(SceneEffectCoroutine);

        if (_animator == null)
        {
            Debug.LogWarning("<b>[SceneEffect]</b> Animator is missing!");
            yield break;
        }

        DisableAllInput();

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");

        yield return new WaitForSeconds(_nextSceneEffect.length);

        ReleaseAllPoolObjects();

        SceneManager.LoadScene(SceneBuildIndex);

        yield return new WaitForSeconds(_nextSceneEffect.length);

        EventBus.Instance.Publish(new LoadSceneEvent(false));

        EnableAllInput();

        SceneEffectCoroutine = null;
    }

    private IEnumerator QuitGameRoutine()
    {
        if (SceneEffectCoroutine != null)
        {
            Debug.LogWarning("<r>[SceneEffect]</r> Duplicate Coroutine");
            yield break;
        }

        DisableAllInput();

        _animator.SetTrigger(_nextSceneEffect.name);

        EventBus.Instance.Publish(new LoadSceneEvent(true));

        Debug.Log($"<b><color=orange>[SceneEffect]</color></b> Playing: {_nextSceneEffect}");

        yield return new WaitForSeconds(_nextSceneEffect.length);

        ReleaseAllPoolObjects();

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

    private void DisableAllInput()
    {
        var playerInput = Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            Debug.Log("<b><color=#FF6B9D>[SceneEffect]</color></b> ⛔ PlayerInput disabled.");
        }
    }

    private void EnableAllInput()
    {
        var playerInput = Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.ActivateInput();
            Debug.Log("<b><color=#69F0AE>[SceneEffect]</color></b> ✅ PlayerInput enabled.");
        }
    }

    private void ReleaseAllPoolObjects()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReleaseAllPools();
            Debug.Log("<b><color=#FFEB3B>[SceneEffect]</color></b> All pool objects returned to pools.");
        }
    }
}