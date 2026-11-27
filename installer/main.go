package main

import (
	"bufio"
	"bytes"
	_ "embed"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
)

//go:embed catalog.json
var catalog []byte

type Package struct {
	Name    string `json:"name"`
	ID      string `json:"id"`
	Profile string `json:"profile"`
	AlsoProfiles []string `json:"also_profiles"`
	URL     string `json:"url"`
}

func (p Package) selected(pick, profile string) bool {
	if pick == "1" { return true }
	if p.Profile == "optional" || p.Profile == "manual" { return false }
	if pick == "5" || p.Profile == profile { return true }
	for _, additional := range p.AlsoProfiles {
		if additional == profile { return true }
	}
	return false
}

func main() { os.Exit(run()) }
func run() int {
	r := bufio.NewReader(os.Stdin)
	ask := func(s string) string { fmt.Print(s); v, _ := r.ReadString('\n'); return strings.TrimSpace(v) }
	var pkgs []Package
	if err := json.Unmarshal(catalog, &pkgs); err != nil {
		fmt.Println(err)
		return 2
	}
	fmt.Println("VELTRIVON | Windows Control Suite | Utility installer\n1 Search catalog only\n2 Install essentials\n3 Install extended tools\n4 Install XR/media tools\n5 Install all installation profiles\n6 Review optional/manual tools\n7 Exit\n8 Install essential developer tools\n9 Install open-source-first XR tools")
	pick := ask("Choose 1-9: ")
	if pick == "7" { return 0 }
	if pick == "6" {
		for _, p := range pkgs {
			if p.Profile == "optional" || p.Profile == "manual" { fmt.Printf("%s: %s\n", p.Name, p.URL) }
		}
		ask("Press Enter to exit.")
		return 0
	}
	profiles := map[string]string{"2": "essential", "3": "extended", "4": "xr", "8": "developer", "9": "xr-open"}
	if pick != "1" && pick != "5" && profiles[pick] == "" {
		fmt.Println("Invalid selection")
		return 2
	}
	path, err := exec.LookPath("winget.exe")
	if err != nil {
		fmt.Println("WinGet missing. Install Microsoft App Installer: https://learn.microsoft.com/windows/package-manager/winget/")
		ask("Press Enter to exit.")
		return 2
	}
	if pick != "1" {
		for _, p := range pkgs {
			if p.selected(pick, profiles[pick]) { fmt.Printf("%s [%s]\n", p.Name, p.ID) }
		}
		if !strings.EqualFold(ask("Install selected tools and accept package/source agreements? [y/N]: "), "y") { return 0 }
	}
	base := os.Getenv("LOCALAPPDATA")
	if base == "" { base = os.TempDir() }
	dir := filepath.Join(base, "Veltrivon", "Installer")
	if err := os.MkdirAll(dir, 0700); err != nil { fmt.Println(err); return 2 }
	f, err := os.CreateTemp(dir, "install-*.log")
	if err != nil { fmt.Println(err); return 2 }
	defer f.Close()
	w := io.MultiWriter(os.Stdout, f)
	call := func(args ...string) (string, error) {
		var b bytes.Buffer
		c := exec.Command(path, args...)
		c.Stdout = io.MultiWriter(w, &b)
		c.Stderr = w
		c.Stdin = os.Stdin
		err := c.Run()
		return b.String(), err
	}
	_, _ = call("source", "update", "--name", "winget")
	ok, fail, manual := 0, 0, 0
	for _, p := range pkgs {
		if !p.selected(pick, profiles[pick]) { continue }
		fmt.Fprintf(w, "\n[%s] %s\n", p.Profile, p.Name)
		if p.ID == "" { fmt.Fprintln(w, "Manual:", p.URL); manual++; continue }
		args := []string{"--id", p.ID, "--exact", "--source", "winget", "--accept-source-agreements"}
		if _, err := call(append([]string{"search"}, args...)...); err != nil {
			fmt.Fprintln(w, "Unresolved:", p.URL)
			fail++
			continue
		}
		if pick == "1" { ok++; continue }
		out, err := call(append([]string{"list"}, args...)...)
		if err == nil && strings.Contains(strings.ToLower(out), strings.ToLower(p.ID)) {
			fmt.Fprintln(w, "Already installed; skipped")
			ok++
			continue
		}
		if _, err := call(append(append([]string{"install"}, args...), "--accept-package-agreements")...); err != nil {
			fmt.Fprintln(w, "Install failed; official link:", p.URL)
			fail++
			continue
		}
		ok++
	}
	fmt.Fprintf(w, "\nResolved/success: %d | Failed: %d | Manual: %d\nLog: %s\n", ok, fail, manual, f.Name())
	ask("Press Enter to exit.")
	if fail > 0 { return 1 }
	return 0
}
