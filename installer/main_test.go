package main

import (
	"encoding/json"
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
		case "essential", "extended", "xr", "optional", "manual":
		default:
			t.Fatalf("unknown profile %s", v.Profile)
		}
	}
}
