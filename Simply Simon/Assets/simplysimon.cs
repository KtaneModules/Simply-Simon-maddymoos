using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using SolveAnims;
public class simplysimon : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMSelectable[] Buttons;
    public KMSelectable ModuleSelectable;
    public Renderer[] a;
    public Light[] v;

    private readonly Data data = new Data();

    private int[] whyyyyyy = { 1, 2, 3, 4 };

    private string cols = "RGBY";
    private string REFBUT = "RGBY";
    private string ans, input = "", flsh;
    private bool inpttin, inpttbl, solved, okinputs;
    static private int _moduleIdCounter = 1;
    private int _moduleId, _stagesDone;
    private bool focused = false;

    Dictionary<char, Color32> cowowl = new Dictionary<char, Color32>()
    {
    { 'R', new Color32(255, 0, 0, 155) },
    { 'G', new Color32(0, 255, 0, 155) },
    { 'B', new Color32(0, 0, 255, 155) },
    { 'Y', new Color32(255, 255, 0, 155) }
    };
    void Awake()
    {
        _moduleId = _moduleIdCounter++;
        for(int i = 0; i < 4; i++)
        {
            float scalar = transform.lossyScale.x;
            v[i].range *= scalar;
        }
        for(byte i = 0; i < Buttons.Length; i++)
        {
            KMSelectable btn = Buttons[i];
            btn.OnInteract += delegate
            {
                HandlePress(btn);
                return false;
            };
        }
        if(Application.isEditor)
        {
            focused = true;
        }
        ModuleSelectable.OnFocus += delegate () { focused = true; };
        ModuleSelectable.OnDefocus += delegate () { focused = false; };
    }
    // Use this for initialization
    void Start()
    {
        _stagesDone = 0;
        whyyyyyy = whyyyyyy.Shuffle();
        REFBUT = REFBUT.ToList().Shuffle().Join("");
        cols = REFBUT;
#if UNITY_EDITOR
        _stagesDone = 4;
        cols = cols.ToArray().Shuffle().Join("");
        Debug.LogFormat("[Simply Simon #{0}]: The sequence is now {1}.", _moduleId, cols);
#endif
        Debug.LogFormat("[Simply Simon #{0}]: The button's colors are {1}.", _moduleId, REFBUT);
        GenerateStage();
        for(int i = 0; i < 4; i++)
        {
            a[i].material.SetColor("_Color", cowowl[REFBUT[i]]);
            v[i].color = cowowl[REFBUT[i]];
        }
    }

    // Update is called once per frame
    void HandlePress(KMSelectable btn)
    {
        int x = Array.IndexOf(Buttons, btn);
        if(inpttbl && !solved)
        {
            StopAllCoroutines();
            inpttin = true;
            if(REFBUT[x] == ans[input.Length])
            {
                input += REFBUT[x];
                Debug.LogFormat("[Simply Simon #{0}]: Correctly pressed {1}.", _moduleId, input.Last());
                if(input == ans && ans == flsh)
                {
                    Debug.LogFormat("[Simply Simon #{0}]: Simple. Simon. Solved.", _moduleId);
                    StartCoroutine(Flash2(x, false));
                    StartCoroutine(Flash3());
                    Module.HandlePass();
                    inpttin = false;
                    input = "";
                    solved = true;
                }
                else if(input == ans)
                {
                    Debug.LogFormat("[Simply Simon #{0}]: Correctly inputted sequence. Advancing to the next.", _moduleId);
                    StartCoroutine(Flash2(x, true));
                    inpttin = false;
                    input = "";
                }
                else
                {
                    StartCoroutine(Flash2(x, false));
                }
            }
            else
            {
                Debug.LogFormat("[Simply Simon #{0}]: You pressed {1} when I wanted {2}. Resetting...", _moduleId, REFBUT[x], ans[input.Length]);
                Module.HandleStrike();
                inpttbl = false;
                inpttin = false;
                okinputs = false;
                Start();
                input = "";
            }
        }
    }
    void GenerateStage(bool checkCap = true)
    {
        whyyyyyy = whyyyyyy.Shuffle();
        flsh = "";
        ans = "";
        if(checkCap && _stagesDone >= Rnd.Range(2, 5))
        {
            GenerateCappedStage();
            return;
        }
        for(int i = 0; i <= Rnd.Range(1, 4); i++)
        {
            flsh += REFBUT[Rnd.Range(0, 4)];
            switch(flsh[i])
            {
                case 'R': cols = "" + cols[3] + cols[0] + cols[1] + cols[2]; break;
                case 'B': cols = "" + cols[2] + cols[3] + cols[0] + cols[1]; break;
                case 'Y': cols = "" + cols[1] + cols[0] + cols[3] + cols[2]; break;
                case 'G': cols = "" + cols[3] + cols[2] + cols[1] + cols[0]; break;
            }
        }
        for(int i = 0; i < flsh.Length; i++)
        {
            ans += REFBUT[Array.IndexOf(cols.ToArray(), flsh[i])];
        }
        Debug.LogFormat("[Simply Simon #{0}]: The flashes are {1}.", _moduleId, flsh);
        Debug.LogFormat("[Simply Simon #{0}]: The sequence is now {1}.", _moduleId, cols);
        Debug.LogFormat("[Simply Simon #{0}]: The answer is now {1}.", _moduleId, ans);
        StartCoroutine(Flash());
        _stagesDone++;
    }

    private void GenerateCappedStage()
    {
        Dictionary<string, string> attempts = new Dictionary<string, string>()
        {
            {"", "" + cols[0] + cols[1] + cols[2] + cols[3] }
        };

        int len = 0;

        while(len < 4)
        {
            if(len > 2)
            {
                foreach(KeyValuePair<string, string> kvp in attempts)
                {
                    if(kvp.Key.All(c => REFBUT[Array.IndexOf(kvp.Value.ToArray(), c)] == c))
                    {
                        ans = flsh = kvp.Key;
                        cols = kvp.Value;
                        goto good;
                    }
                }
            }

            Dictionary<string, string> temp = new Dictionary<string, string>();

            foreach(KeyValuePair<string, string> kvp in attempts)
            {
                temp.Add(kvp.Key + "R", "" + kvp.Value[3] + kvp.Value[0] + kvp.Value[1] + kvp.Value[2]);
                temp.Add(kvp.Key + "B", "" + kvp.Value[2] + kvp.Value[3] + kvp.Value[0] + kvp.Value[1]);
                temp.Add(kvp.Key + "Y", "" + kvp.Value[1] + kvp.Value[0] + kvp.Value[3] + kvp.Value[2]);
                temp.Add(kvp.Key + "G", "" + kvp.Value[3] + kvp.Value[2] + kvp.Value[1] + kvp.Value[0]);
            }

            attempts = temp;

            len++;
        }
        _stagesDone = 3;
        Debug.LogFormat("<Simply Simon #{0}>: Stage cap failed! Generating normally.", _moduleId);
        GenerateStage(false);
        return;
        good:

        Debug.LogFormat("[Simply Simon #{0}]: The flashes are {1}.", _moduleId, flsh);
        Debug.LogFormat("[Simply Simon #{0}]: The sequence is now {1}.", _moduleId, cols);
        Debug.LogFormat("[Simply Simon #{0}]: The answer is now {1}.", _moduleId, ans);
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        while(!inpttin)
        {
            for(int i = 0; i < flsh.Length; i++)
            {
                yield return new WaitForSeconds(.2f);
                v[Array.IndexOf(REFBUT.ToArray(), flsh[i])].enabled = true;
                if(focused) Audio.PlaySoundAtTransform(GetSound(Array.IndexOf(REFBUT.ToArray(), flsh[i])), Buttons[Array.IndexOf(REFBUT.ToArray(), flsh[i])].transform);
                yield return new WaitForSeconds(.3f);
                v[Array.IndexOf(REFBUT.ToArray(), flsh[i])].enabled = false;
            }
            inpttbl = true;
            yield return new WaitForSeconds(1f);
            inpttbl = false;
        }
    }
    IEnumerator Flash2(int index, bool generate)
    {
        inpttbl = false;
        Audio.PlaySoundAtTransform(GetSound(index), Buttons[index].transform);
        v[index].enabled = true;
        yield return new WaitForSeconds(.3f);
        v[0].enabled = false;
        v[1].enabled = false;
        v[2].enabled = false;
        v[3].enabled = false;
        inpttbl = true;
        if(generate)
        {
            yield return new WaitForSeconds(1f);
            GenerateStage();
        }
    }
    IEnumerator Flash3()
    {
        yield return new WaitForSeconds(.5f);
        inpttbl = false;
        Audio.PlaySoundAtTransform(GetSound(0), Buttons[0].transform);
        Audio.PlaySoundAtTransform(GetSound(1), Buttons[1].transform);
        Audio.PlaySoundAtTransform(GetSound(2), Buttons[2].transform);
        Audio.PlaySoundAtTransform(GetSound(3), Buttons[3].transform);
        a[0].material.SetColor("_Color", cowowl['G']);
        v[0].color = cowowl['G'];
        a[1].material.SetColor("_Color", cowowl['Y']);
        v[1].color = cowowl['Y'];
        a[2].material.SetColor("_Color", cowowl['R']);
        v[2].color = cowowl['R'];
        a[3].material.SetColor("_Color", cowowl['B']);
        v[3].color = cowowl['B'];
        whyyyyyy = new int[] { 1, 2, 3, 4 };
        yield return new WaitForSeconds(1f);
        while(true)
        {
            string AnimAnim = data[Rnd.Range(0, data.Length)];
            for(int i = 0; i < AnimAnim.Length; i++)
            {
                v[int.Parse(AnimAnim[i].ToString()) - 1].enabled = !v[int.Parse(AnimAnim[i].ToString()) - 1].enabled;
                if(v[int.Parse(AnimAnim[i].ToString()) - 1].enabled == true)
                {
                    if(focused) Audio.PlaySoundAtTransform(GetSound(int.Parse(AnimAnim[i].ToString()) - 1), Buttons[int.Parse(AnimAnim[i].ToString()) - 1].transform);
                }
                yield return new WaitForSeconds(.0857f);
            }
        }
    }
    private string GetSound(int i)
    {
        int aa = whyyyyyy[i];
        return "simon" + aa;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        int i = 0;
        while(!solved)
        {
            while(!inpttbl)
            {
                yield return new WaitForSeconds(.01f);
                i = 0;
            }
            yield return new WaitForSeconds(.5f);
            while(i < ans.Length)
            {
                Buttons[Array.IndexOf(REFBUT.ToArray(), ans[i])].OnInteract();
                yield return new WaitForSeconds(.5f);
                i++;
            }
            inpttbl = false;
            yield return new WaitForSeconds(1.2f);
            i = 0;
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} RGBYY (Presses Red, Green, Blue, Yellow, and Yellow)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if((m = Regex.Match(command, @"^\s*([RGBY]*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            string owo = m.Groups[1].Value.Join("").ToUpper();
            yield return null;
            okinputs = true;
            int i = 0;
            if(ans == flsh)
            {
                yield return "solve";
            }
            while(!inpttbl)
            {
                yield return new WaitForSeconds(.1f);
            }
            while(okinputs && i < owo.Length)
            {
                Buttons[Array.IndexOf(REFBUT.ToArray(), owo[i])].OnInteract();
                i++;
                yield return new WaitForSeconds(.5f);
            }
        }
        else
            yield return "sendtochaterror Incorrect Syntax. Use '!{1} RGBYY'.";
    }

}
