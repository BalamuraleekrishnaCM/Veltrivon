package main

import (
	"encoding/json"
	"os"
	"regexp"
	"strings"
	"testing"
)

func TestCatalog(t *testing.T) {
	var p []Package
	if err := json.Unmarshal(catalog, &p); err != nil {
		t.Fatal(err)
	}
	seen := map[string]bool{}
	for _, v := range p {
		if v.Name == "" || !strings.HasPrefix(v.URL, "https://") {
			t.Fatalf("invalid record %+v", v)
		}
		if v.ID == "" && v.Profile != "manual" {
			t.Fatalf("missing id %+v", v)
		}
		id := strings.ToLower(v.ID)
		if id != "" && seen[id] {
			t.Fatalf("duplicate %s", id)
		}
		seen[id] = true
		switch v.Profile {
		case "essential", "extended", "xr", "developer", "xr-open", "optional", "manual":
		default:
			t.Fatalf("unknown profile %s", v.Profile)
		}
		for _, profile := range v.AlsoProfiles {
			if profile == v.Profile || (profile != "developer" && profile != "xr-open") || !v.selected("8", profile) {
				t.Fatalf("invalid additional profile %q in %+v", profile, v)
			}
		}
	}
}

func TestBatchProfilesMatchCatalog(t *testing.T) {
	var packages []Package
	if err := json.Unmarshal(catalog, &packages); err != nil {
		t.Fatal(err)
	}
	batch, err := os.ReadFile("Veltrivon-Install.bat")
	if err != nil {
		t.Fatal(err)
	}
	rows := regexp.MustCompile(`(?m)^if "%PICK%"=="([1-9])" call :pkg "[^"]+" "([^"]+)"`)
	profiles := map[string]string{
		"1": "", "2": "essential", "3": "extended", "4": "xr", "5": "",
		"8": "developer", "9": "xr-open",
	}
	seen := map[string]map[string]bool{}
	for _, row := range rows.FindAllStringSubmatch(string(batch), -1) {
		pick, id := row[1], row[2]
		if seen[pick] == nil {
			seen[pick] = map[string]bool{}
		}
		if seen[pick][id] {
			t.Fatalf("duplicate batch package %s in choice %s", id, pick)
		}
		seen[pick][id] = true
	}
	for pick, profile := range profiles {
		for _, pkg := range packages {
			if pkg.ID == "" {
				continue
			}
			if seen[pick][pkg.ID] != pkg.selected(pick, profile) {
				t.Errorf("batch and catalog disagree on %s for choice %s", pkg.ID, pick)
			}
			delete(seen[pick], pkg.ID)
		}
		for id := range seen[pick] {
			t.Errorf("unknown batch package %s for choice %s", id, pick)
		}
	}
}
